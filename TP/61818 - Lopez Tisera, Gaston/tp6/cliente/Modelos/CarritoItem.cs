namespace Carrito.Modelos
{
    public class CarritoItem
    {
        public Producto Producto { get; set; } = new();
        public int Cantidad { get; set; }
    }
}