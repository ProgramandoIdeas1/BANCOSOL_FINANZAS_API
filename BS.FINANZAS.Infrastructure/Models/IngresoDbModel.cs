namespace BS.FINANZAS.Infrastructure.Models
{
    public class IngresoDbModel
    {
        public int Id { get; set; }
        public decimal Monto { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Fuente { get; set; } = string.Empty;
        public string Moneda { get; set; } = string.Empty;
    }
}
