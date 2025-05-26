using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

var apiUrl = "http://localhost:5000";
var clienteHttp = new HttpClient();
var opcionesJson = new JsonSerializerOptions {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true
};

async Task<List<ItemArticulo>> ObtenerArticulosAsync() {
    var json = await clienteHttp.GetStringAsync($"{apiUrl}/productos");
    return JsonSerializer.Deserialize<List<ItemArticulo>>(json, opcionesJson)!;
}

async Task<List<ItemArticulo>> ObtenerArticulosBajoStockAsync() {
    var json = await clienteHttp.GetStringAsync($"{apiUrl}/productos/reponer");
    return JsonSerializer.Deserialize<List<ItemArticulo>>(json, opcionesJson)!;
}

async Task AgregarStockAsync(int id, int cantidad) {
    var res = await clienteHttp.PutAsync($"{apiUrl}/productos/{id}/agregar/{cantidad}", null);
    Console.WriteLine(res.IsSuccessStatusCode ? "✔ Stock sumado." : "❌ Error al sumar stock.");
}

async Task QuitarStockAsync(int id, int cantidad) {
    var res = await clienteHttp.PutAsync($"{apiUrl}/productos/{id}/quitar/{cantidad}", null);
    var txt = await res.Content.ReadAsStringAsync();
    Console.WriteLine(res.IsSuccessStatusCode ? "✔ Stock restado." : $"❌ Error: {txt}");
}

int LeerSeleccion(string mensaje, int min, int max) {
    int opcion;
    do {
        Console.Write(mensaje);
    } while (!int.TryParse(Console.ReadLine(), out opcion) || opcion < min || opcion > max);
    return opcion;
}

while (true) {
    Console.WriteLine("\n=== Menú ===");
    Console.WriteLine("1. Listar artículos");
    Console.WriteLine("2. Listar artículos con stock bajo (<3)");
    Console.WriteLine("3. Sumar stock");
    Console.WriteLine("4. Restar stock");
    Console.WriteLine("0. Salir");

    var opcion = LeerSeleccion("Elija una opción: ", 0, 4);
    if (opcion == 0) break;

    if (opcion == 1) {
        foreach (var art in await ObtenerArticulosAsync())
            Console.WriteLine($"{art.Id} {art.Nombre,-20} {art.Stock,3} u. {art.Precio,8:c}");
    } else if (opcion == 2) {
        foreach (var art in await ObtenerArticulosBajoStockAsync())
            Console.WriteLine($"{art.Id} {art.Nombre,-20} {art.Stock,3} u. {art.Precio,8:c}");
    } else if (opcion == 3) {
        Console.Write("ID artículo: "); int id = int.Parse(Console.ReadLine()!);
        Console.Write("Cantidad a sumar: "); int cant = int.Parse(Console.ReadLine()!);
        await AgregarStockAsync(id, cant);
    } else if (opcion == 4) {
        Console.Write("ID artículo: "); int id = int.Parse(Console.ReadLine()!);
        Console.Write("Cantidad a restar: "); int cant = int.Parse(Console.ReadLine()!);
        await QuitarStockAsync(id, cant);
    }
}

class ItemArticulo {
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public decimal Precio { get; set; }
    public int Stock { get; set; }
}