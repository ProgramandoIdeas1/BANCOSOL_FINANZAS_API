namespace BS.FINANZAS.Application.DTOs.Responses
{
    public class BalanceReportResponseDto
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string MonedaResultado { get; set; } = string.Empty;
        public decimal TotalConsolidado { get; set; }
        public decimal TipoCambioUsdBob { get; set; }
        public int TotalIngresosProcesados { get; set; }
    }
}
