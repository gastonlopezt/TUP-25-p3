using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

var apiUrl = "http://localhost:5000";
var clienteHttp = new HttpClient();
var opcionesJson = new JsonSerializerOptions {
    PropertyNamingPolicy        = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true
};

async Task MenuPrincipal()
{
    while (true)
    {
        Console.WriteLine("\n=== Gestión de Inventario ===");
        Console.WriteLine("1. Mostrar artículos en stock");
        Console.WriteLine("2. Mostrar artículos bajos de stock");
        Console.WriteLine("3. Sumar unidades a un artículo");
        Console.WriteLine("4. Restar unidades a un artículo");
        Console.WriteLine("5. Salir");
        Console.Write("Elija una opción: ");
        var eleccion = Console.ReadLine();

        switch (eleccion)
        {
            case "1":
                await MostrarArticulosEnStock();
                break;
            case "2":
                await MostrarArticulosBajoStock();
                break;
            case "3":
                await SumarUnidades();
                break;
            case "4":
                await RestarUnidades();
                break;
            case "5":
                return;
            default:
                Console.WriteLine("Opción incorrecta.");
                break;
        }
    }
}

async Task MostrarArticulosEnStock(){
    Console.WriteLine("\n=== Artículos en Stock ===");
    var articulos = await clienteHttp.GetFromJsonAsync<List<ItemInventario>>($"{apiUrl}/productos", opcionesJson);
    if (articulos is null || !articulos.Any())
    {
        Console.WriteLine("No hay artículos en stock.");
        return;
    }
    foreach (var a in articulos) {
        Console.WriteLine($"{a.Codigo} {a.Descripcion,-20} {a.Valor,15:c} Unidades: {a.Cantidad}");
    }
}

async Task MostrarArticulosBajoStock(){
    Console.WriteLine("\n=== Artículos para Reponer (Stock < 3) ===");
    var articulos = await clienteHttp.GetFromJsonAsync<List<ItemInventario>>($"{apiUrl}/productos/reponer", opcionesJson);
    if (articulos is null || !articulos.Any())
    {
        Console.WriteLine("No hay artículos para reponer.");
        return;
    }
    foreach (var a in articulos) {
        Console.WriteLine($"{a.Codigo} {a.Descripcion,-20} {a.Valor,15:c} Unidades: {a.Cantidad}");
    }
}

async Task SumarUnidades(){
    Console.Write("Ingrese el código del artículo: ");
    if (!int.TryParse(Console.ReadLine(), out int codigo))
    {
        Console.WriteLine("Código no válido.");
        return;
    }
    Console.Write("Ingrese la cantidad a sumar: ");
    if (!int.TryParse(Console.ReadLine(), out int unidades) || unidades <= 0)
    {
        Console.WriteLine("Cantidad no válida.");
        return;
    }

    var respuesta = await clienteHttp.PostAsJsonAsync($"{apiUrl}/productos/{codigo}/agregar-stock", new UnidadesDto { Unidades = unidades }, opcionesJson);
    if (respuesta.IsSuccessStatusCode)
    {
        var articulo = await respuesta.Content.ReadFromJsonAsync<ItemInventario>(opcionesJson);
        Console.WriteLine($"Unidades sumadas. Nuevo stock de {articulo?.Descripcion}: {articulo?.Cantidad}");
    }
    else
    {
        Console.WriteLine($"Error al sumar unidades: {respuesta.ReasonPhrase}");
    }
}

async Task RestarUnidades(){
    Console.Write("Ingrese el código del artículo: ");
    if (!int.TryParse(Console.ReadLine(), out int codigo))
    {
        Console.WriteLine("Código no válido.");
        return;
    }
    Console.Write("Ingrese la cantidad a restar: ");
    if (!int.TryParse(Console.ReadLine(), out int unidades) || unidades <= 0)
    {
        Console.WriteLine("Cantidad no válida.");
        return;
    }

    var respuesta = await clienteHttp.PostAsJsonAsync($"{apiUrl}/productos/{codigo}/quitar-stock", new UnidadesDto { Unidades = unidades }, opcionesJson);
    if (respuesta.IsSuccessStatusCode)
    {
        var articulo = await respuesta.Content.ReadFromJsonAsync<ItemInventario>(opcionesJson);
        Console.WriteLine($"Unidades restadas. Nuevo stock de {articulo?.Descripcion}: {articulo?.Cantidad}");
    }
    else
    {
        var error = await respuesta.Content.ReadAsStringAsync();
        Console.WriteLine($"Error al restar unidades: {respuesta.ReasonPhrase} - {error}");
    }
}

await MenuPrincipal();

class ItemInventario {
    public int Codigo { get; set; }
    public string Descripcion { get; set; } = null!;
    public decimal Valor { get; set; }
    public int Cantidad { get; set; }
}

class UnidadesDto
{
    public int Unidades { get; set; }
}