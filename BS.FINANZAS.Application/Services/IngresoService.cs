using BS.FINANZAS.Application.DTOs.Requests;
using BS.FINANZAS.Application.DTOs.Responses;
using BS.FINANZAS.Application.Interfaces;
using BS.FINANZAS.Domain.Entities;
using BS.FINANZAS.Domain.Interfaces;

namespace BS.FINANZAS.Application.Services
{
    public class IngresoService : IIngresoService
    {
        private readonly IIngresoRepository _ingresoRepository;
        private readonly IHexaRateService _hexaRateService;
        public IngresoService(IIngresoRepository ingresoRepository, IHexaRateService hexaRateService)
        {
            _ingresoRepository = ingresoRepository;
            _hexaRateService = hexaRateService;
        }

        public async Task<IngresoResponseDto> RegistrarIngresoAsync(CrearIngresoRequestDto dto)
        {
            if (!Enum.TryParse<Moneda>(dto.Moneda, true, out var monedaParsed) || !Enum.IsDefined(typeof(Moneda), monedaParsed))
            {
                throw new ArgumentException($"La moneda '{dto.Moneda}' no esta soportada. Monedas válidas: BOB, USD.");
            }

            var nuevoIngreso = new Ingreso
            {
                Monto = dto.Monto,
                Descripcion = dto.Descripcion,
                Fecha = dto.Fecha,
                Fuente = dto.Fuente,
                Moneda = monedaParsed
            };

            var creado = await _ingresoRepository.AgregarAsync(nuevoIngreso);
            return MapToDto(creado);
        }

        public async Task<IEnumerable<IngresoResponseDto>> ObtenerIngresosAsync()
        {
            var ingresos = await _ingresoRepository.ObtenerIngresosAsync();
            return ingresos.Select(MapToDto);
        }

        public async Task<IngresoResponseDto?> ObtenerPorIdAsync(int id)
        {
            var ingreso = await _ingresoRepository.ObtenerPorIdAsync(id);
            return ingreso is null ? null : MapToDto(ingreso);
        }

        public async Task<decimal> ObtenerTipoCambioAsync()
        {
            return await _hexaRateService.ObtenerTipoCambioUsdBobAsync();
        }

        public async Task<BalanceReportResponseDto> CalcularBalanceConsolidadoAsync(DateTime fechaInicio, DateTime fechaFin, string monedaSolicitada)
        {
            if (!Enum.TryParse<Moneda>(monedaSolicitada, true, out var monedaTarget) || !Enum.IsDefined(typeof(Moneda), monedaTarget))
            {
                throw new ArgumentException($"La moneda solicita '{monedaSolicitada}' no esta soportada. Monedas válidas: BOB, USD.");
            }

            var ingresos = await _ingresoRepository.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin);
            var tasa = await _hexaRateService.ObtenerTipoCambioUsdBobAsync();

            decimal totalConsolidado = 0m;
            int totalProcesados = 0;

            foreach (var ingreso in ingresos)
            {
                totalProcesados++;

                if (monedaTarget == Moneda.BOB)
                {
                    if (ingreso.Moneda == Moneda.BOB)
                    {
                        totalConsolidado += ingreso.Monto;
                    }
                    else if (ingreso.Moneda == Moneda.USD)
                    {
                        totalConsolidado += ingreso.Monto * tasa;
                    }
                }
                
                else if (monedaTarget == Moneda.USD)
                {
                    if (ingreso.Moneda == Moneda.USD)
                    {
                        totalConsolidado += ingreso.Monto;
                    }
                    else if (ingreso.Moneda == Moneda.BOB)
                    {
                        totalConsolidado += ingreso.Monto / tasa;
                    }
                }
            }

            return new BalanceReportResponseDto
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                MonedaResultado = monedaTarget.ToString(),
                TotalConsolidado = Math.Round(totalConsolidado, 2),
                TipoCambioUsdBob = tasa,
                TotalIngresosProcesados = totalProcesados
            };
        }

        private static IngresoResponseDto MapToDto(Ingreso ingreso)
        {
            return new IngresoResponseDto
            {
                Id = ingreso.Id,
                Monto = ingreso.Monto,
                Descripcion = ingreso.Descripcion,
                Fecha = ingreso.Fecha,
                Fuente = ingreso.Fuente,
                Moneda = ingreso.Moneda.ToString()
            };
        }
    }
}
