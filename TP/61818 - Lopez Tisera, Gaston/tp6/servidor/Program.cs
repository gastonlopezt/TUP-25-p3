using Microsoft.EntityFrameworkCore;
using Servidor.Datos;
using Servidor.Modelos;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TiendaDbContext>(options =>
    options.UseSqlite("Data Source=tienda.db"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TiendaDbContext>();
    db.Database.EnsureCreated();

    if (!db.Productos.Any())
    {
        db.Productos.AddRange(new List<Producto>
        {
            new() { Nombre = "Celular A1", Descripcion = "Smartphone básico", Precio = 50000, Stock = 20, ImagenUrl = "img/celular1.png" },
            new() { Nombre = "Celular B2", Descripcion = "Smartphone gama media", Precio = 75000, Stock = 15, ImagenUrl = "img/celular2.png" },
            new() { Nombre = "Celular C3", Descripcion = "Gama alta", Precio = 120000, Stock = 10, ImagenUrl = "img/celular3.png" },
            new() { Nombre = "Auriculares X", Descripcion = "Bluetooth", Precio = 15000, Stock = 30, ImagenUrl = "img/auriculares1.png" },
            new() { Nombre = "Funda resistente", Descripcion = "Funda para celular", Precio = 5000, Stock = 25, ImagenUrl = "img/funda1.png" },
            new() { Nombre = "Cargador rápido", Descripcion = "Cargador USB-C", Precio = 7000, Stock = 20, ImagenUrl = "img/cargador1.png" },
            new() { Nombre = "Smartwatch Y", Descripcion = "Reloj inteligente", Precio = 45000, Stock = 12, ImagenUrl = "img/watch1.png" },
            new() { Nombre = "Notebook Z", Descripcion = "Laptop liviana", Precio = 220000, Stock = 5, ImagenUrl = "img/laptop1.png" },
            new() { Nombre = "Tablet Mini", Descripcion = "Pantalla 8\"", Precio = 90000, Stock = 8, ImagenUrl = "img/tablet1.png" },
            new() { Nombre = "Teclado gamer", Descripcion = "RGB mecánico", Precio = 25000, Stock = 14, ImagenUrl = "img/teclado1.png" },
        });

        db.SaveChanges();
    }
}
app.MapGet("/test-productos", async (TiendaDbContext db) =>
{
    var productos = await db.Productos.ToListAsync();
    return Results.Ok(productos);
});

app.Run();

// // Agregar servicios CORS para permitir solicitudes desde el cliente
// builder.Services.AddCors(options => {
//     options.AddPolicy("AllowClientApp", policy => {
//         policy.WithOrigins("http://localhost:5177", "https://localhost:7221")
//               .AllowAnyHeader()
//               .AllowAnyMethod();
//     });
// });