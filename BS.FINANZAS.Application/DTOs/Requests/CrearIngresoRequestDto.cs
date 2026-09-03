using System.ComponentModel.DataAnnotations;

namespace BS.FINANZAS.Application.DTOs.Requests
{
    public class CrearIngresoRequestDto
    {
        [Required(ErrorMessage = "El monto es requerido.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero 0.")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "La descripcion es requerida.")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha es requerida.")]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "La fuente es requerida.")]
        public string Fuente { get; set; } = string.Empty;

        [Required(ErrorMessage = "La moneda es requerida.")]
        public string Moneda { get; set; } = string.Empty;
    }
}
