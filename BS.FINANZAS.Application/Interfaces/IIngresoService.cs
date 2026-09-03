using BS.FINANZAS.Application.DTOs.Requests;
using BS.FINANZAS.Application.DTOs.Responses;

namespace BS.FINANZAS.Application.Interfaces
{
    public interface IIngresoService
    {
        Task<IngresoResponseDto> RegistrarIngresoAsync(CrearIngresoRequestDto dto);
        Task<IEnumerable<IngresoResponseDto>> ObtenerIngresosAsync();
        Task<IngresoResponseDto?> ObtenerPorIdAsync(int id);
        Task<decimal> ObtenerTipoCambioAsync();
        Task<BalanceReportResponseDto> CalcularBalanceConsolidadoAsync(DateTime fechaInicio, DateTime fechaFin, string monedaSolicitada);
    }
}
