using System.ComponentModel.DataAnnotations;

namespace BS.FINANZAS.Application.DTOs.Requests
{
    public class BalanceReportRequestDto
    {
        [Required]
        public DateTime FechaInicio { get; set; }
        [Required]
        public DateTime FechaFin { get; set; }
        [Required]
        public string Moneda { get; set; } = string.Empty;
    }
}
