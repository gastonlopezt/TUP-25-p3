using Microsoft.EntityFrameworkCore;
using Servidor.Datos;
using Servidor.Modelos;
using Tienda.Modelos;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TiendaDbContext>(options =>
    options.UseSqlite("Data Source=tienda.db"));

var app = builder.Build();
// var testCarrito = new Tienda.Modelos.Carrito();
var carritos = new Dictionary<Guid, Carrito>();

app.MapGet("/productos", async (TiendaDbContext db, string? query) =>
{
    var productos = string.IsNullOrWhiteSpace(query)
        ? await db.Productos.ToListAsync()
        : await db.Productos
            .Where(p => p.Nombre.Contains(query) || p.Descripcion.Contains(query))
            .ToListAsync();

    return Results.Ok(productos);
});
app.MapPost("/carritos", () =>
{
    var carrito = new Carrito();
    carritos[carrito.Id] = carrito;
    return Results.Ok(carrito.Id);
});

app.MapGet("/carritos/{id:guid}", (Guid id) =>
{
    if (carritos.TryGetValue(id, out var carrito))
        return Results.Ok(carrito);

    return Results.NotFound();
});

app.MapDelete("/carritos/{id:guid}", (Guid id) =>
{
    if (carritos.Remove(id))
        return Results.Ok();

    return Results.NotFound();
});

app.MapPut("/carritos/{id:guid}/{productoId:int}", async (Guid id, int productoId, TiendaDbContext db) =>
{
    if (!carritos.TryGetValue(id, out var carrito))
        return Results.NotFound("Carrito no encontrado");

    var producto = await db.Productos.FindAsync(productoId);
    if (producto == null || producto.Stock <= 0)
        return Results.BadRequest("Producto no disponible");

    var item = carrito.Items.FirstOrDefault(i => i.ProductoId == productoId);
    if (item != null)
    {
        if (producto.Stock <= item.Cantidad)
            return Results.BadRequest("No hay stock disponible para aumentar la cantidad");

        item.Cantidad++;
    }
    else
    {
        carrito.Items.Add(new ItemCarrito
        {
            ProductoId = producto.Id,
            Nombre = producto.Nombre,
            Cantidad = 1,
            PrecioUnitario = producto.Precio
        });
    }

    return Results.Ok(carrito);
});

app.MapDelete("/carritos/{id:guid}/{productoId:int}", (Guid id, int productoId) =>
{
    if (!carritos.TryGetValue(id, out var carrito))
        return Results.NotFound("Carrito no encontrado");

    var item = carrito.Items.FirstOrDefault(i => i.ProductoId == productoId);
    if (item == null)
        return Results.NotFound("Producto no encontrado en carrito");

    item.Cantidad--;
    if (item.Cantidad <= 0)
        carrito.Items.Remove(item);

    return Results.Ok(carrito);
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