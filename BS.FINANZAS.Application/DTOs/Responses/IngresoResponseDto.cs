namespace BS.FINANZAS.Application.DTOs.Responses
{
    public class IngresoResponseDto
    {
        public int Id { get; set; }
        public decimal Monto { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Fuente { get; set; } = string.Empty;
        public string Moneda { get; set; } = string.Empty;
    }
}
