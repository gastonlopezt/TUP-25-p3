#r "sdk:Microsoft.NET.Sdk.Web"
#r "nuget: Microsoft.EntityFrameworkCore, 9.0.4"
#r "nuget: Microsoft.EntityFrameworkCore.Sqlite, 9.0.4"

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

var builder = WebApplication.CreateBuilder();
builder.Services.AddDbContext<TiendaDb>(opt => opt.UseSqlite("Data Source=./tienda.db"));
builder.Services.Configure<JsonOptions>(opt => opt.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);
var app = builder.Build();

app.MapGet("/productos", async (TiendaDb db) => await db.Articulos.ToListAsync());

app.MapGet("/productos/reponer", async (TiendaDb db) =>
    await db.Articulos.Where(p => p.Stock < 3).ToListAsync());

app.MapPut("/productos/{id}/agregar/{cantidad}", async (int id, int cantidad, TiendaDb db) =>
{
    var articulo = await db.Articulos.FindAsync(id);
    if (articulo is null) return Results.NotFound();
    articulo.Stock += cantidad;
    await db.SaveChangesAsync();
    return Results.Ok(articulo);
});

app.MapPut("/productos/{id}/quitar/{cantidad}", async (int id, int cantidad, TiendaDb db) =>
{
    var articulo = await db.Articulos.FindAsync(id);
    if (articulo is null) return Results.NotFound();
    if (articulo.Stock < cantidad) return Results.BadRequest("Stock insuficiente");
    articulo.Stock -= cantidad;
    await db.SaveChangesAsync();
    return Results.Ok(articulo);
});

using (var scope = app.Services.CreateScope())
{
    var dbInit = scope.ServiceProvider.GetRequiredService<TiendaDb>();
    dbInit.Database.EnsureCreated();

    if (!dbInit.Articulos.Any()) {
        dbInit.Articulos.AddRange(
            new Articulo { Nombre = "Arroz", Precio = 150, Stock = 10 },
            new Articulo { Nombre = "Fideos", Precio = 100, Stock = 10 },
            new Articulo { Nombre = "Leche", Precio = 250, Stock = 10 },
            new Articulo { Nombre = "Yerba", Precio = 400, Stock = 10 },
            new Articulo { Nombre = "Galletas", Precio = 120, Stock = 10 },
            new Articulo { Nombre = "Azúcar", Precio = 130, Stock = 10 },
            new Articulo { Nombre = "Café", Precio = 500, Stock = 10 },
            new Articulo { Nombre = "Aceite", Precio = 700, Stock = 10 },
            new Articulo { Nombre = "Sal", Precio = 80, Stock = 10 },
            new Articulo { Nombre = "Harina", Precio = 90, Stock = 10 }
        );
        dbInit.SaveChanges();
    }
}

app.Run("http://localhost:5000");

class TiendaDb : DbContext {
    public TiendaDb(DbContextOptions<TiendaDb> options) : base(options) { }
    public DbSet<Articulo> Articulos => Set<Articulo>();
}

class Articulo {
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public decimal Precio { get; set; }
    public int Stock { get; set; }
}