namespace BS.FINANZAS.Domain.Entities
{
    public enum Moneda
    {
        BOB,
        USD
    }

    public class Ingreso
    {
        public int Id { get; set; }
        public decimal Monto { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Fuente { get; set; } = string.Empty;
        public Moneda Moneda { get; set; }
    }
}
