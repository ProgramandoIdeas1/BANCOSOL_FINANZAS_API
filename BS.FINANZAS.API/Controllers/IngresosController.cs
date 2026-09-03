using BS.FINANZAS.Application.DTOs.Requests;
using BS.FINANZAS.Application.DTOs.Responses;
using BS.FINANZAS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BS.FINANZAS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class IngresosController : Controller
    {
        private readonly IIngresoService _ingresoService;

        public IngresosController(IIngresoService ingresoService)
        {
            _ingresoService = ingresoService;
        }

        /// <summary>
        /// Caso de Uso 1: Registrar un nuevo ingreso (Monedas soportadas: BOB, USD).
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(IngresoResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Registrar([FromBody] CrearIngresoRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var resultado = await _ingresoService.RegistrarIngresoAsync(dto);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Id }, resultado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error interno del servidor.", error = ex.Message });
            }
        }

        /// <summary>
        /// Caso de Uso 2: Consulta de historial completo de ingresos.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<IngresoResponseDto>), statusCode: StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerIngresos()
        {
            try
            {
                var ingresos = await _ingresoService.ObtenerIngresosAsync();
                return Ok(ingresos);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error al obtener ingresos.", error = ex.Message });
            }
        }

        /// <summary>
        /// Caso de Uso 3: Consulta de ingreso específico por su identificador único.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(IngresoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            try
            {
                var ingreso = await _ingresoService.ObtenerPorIdAsync(id);
                if (ingreso is null)
                {
                    return NotFound(new { message = $"No se encontró el ingreso con ID {id}." });
                }
                return Ok(ingreso);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error al obtener el ingreso.", error = ex.Message });
            }
        }

        /// <summary>
        /// Caso de uso 4: Consulta del tipo de cambio actual (HexaRate API)
        /// </summary>
        [HttpGet("tipo-cambio")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerTipoCambio()
        {
            try
            {
                var tasa = await _ingresoService.ObtenerTipoCambioAsync();
                return Ok(new { monedaOrigen = "USD", MonedaDestino = "BOB", TipoCambio = tasa });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error al obtener el tipo de cambio.", error = ex.Message });
            }
        }

        /// <summary>
        /// Caso de Uso 5: Reporte de balance consolidado en un período específico y en la moneda elegida (BOB o USD).
        /// </summary>
        [HttpGet("balance")]
        [ProducesResponseType(typeof(BalanceReportResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ObtenerBalanceConsolidado(
            [FromQuery] DateTime fechaInicio,
            [FromQuery] DateTime fechaFin,
            [FromQuery] string moneda)
        {
            if (fechaInicio > fechaFin)
            {
                return BadRequest(new { message = "La fecha de inicio no puede ser mayor que la fecha de fin." });
            }

            if (string.IsNullOrEmpty(moneda))
            {
                return BadRequest(new { message = "El parámetro 'moneda' es obligatorio (BOB o USD)." });
            }

            try
            {
                var balance = await _ingresoService.CalcularBalanceConsolidadoAsync(fechaInicio, fechaFin, moneda);
                return Ok(balance);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error al calcular el balance.", error = ex.Message });
            }
        }
    }
}