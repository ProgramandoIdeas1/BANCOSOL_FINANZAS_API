using BS.FINANZAS.Application.DTOs.Requests;
using BS.FINANZAS.Application.Interfaces;
using BS.FINANZAS.Application.Services;
using BS.FINANZAS.Domain.Interfaces;
using BS.FINANZAS.Infrastructure.Repositories;
using Moq;

namespace BS.FINANZAS.Tests
{
    public class IngresoServiceTest
    {
        private readonly IIngresoRepository _repository;
        //objeto simulacion mock para el servicio externo tipo de cmabio
        private readonly Mock<IHexaRateService> _mockHexaRateService;
        private readonly IngresoService _ingresoService;
        public IngresoServiceTest()
        {
            //iniciamo un new repository en memoria limpio
            _repository = new InMemoryIngresoRepository();
            _mockHexaRateService = new Mock<IHexaRateService>();
            _ingresoService = new IngresoService(_repository, _mockHexaRateService.Object);
        }

        //para pruebas
        [Fact]
        public async Task RegistrarIngreso_MonedaInvalida_LanzaException()
        {
            //creamos con una moneda no permitida
            var dto = new CrearIngresoRequestDto
            {
                Monto = 100,
                Descripcion = "Pago en euros",
                Fecha = DateTime.Now,
                Fuente = "Freelance",
                Moneda = "EUR" //Moneda no soportada
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _ingresoService.RegistrarIngresoAsync(dto));

            Assert.Contains("La moneda 'EUR' no esta soportada", exception.Message);
        }

        [Fact]
        public async Task CalcularBalanceConsolidado_EnBolivianos_SumaYConvierteCorrectamente()
        {
            // Configuramos el mock para devolver un tipo de cambio fijo
            _mockHexaRateService.Setup(s => s.ObtenerTipoCambioUsdBobAsync()).ReturnsAsync(12.155m);
            // Agregamos ingresos en diferentes monedas

            //fecha fija para la prueba
            var fecha = new DateTime(2025, 12, 15);

            //registramos BOB 3000
            await _ingresoService.RegistrarIngresoAsync(new CrearIngresoRequestDto
            {
                Monto = 3000,
                Descripcion = "Sueldo",
                Fecha = fecha,//dateime.now
                Fuente = "Trabajo",
                Moneda = "BOB"
            });
            //registramos USD 100
            await _ingresoService.RegistrarIngresoAsync(new CrearIngresoRequestDto
            {
                Monto = 100,
                Descripcion = "Pago Freelance",
                Fecha = fecha,
                Fuente = "Freelance",
                Moneda = "USD"
            });
            //registramos BOB 2000
            await _ingresoService.RegistrarIngresoAsync(new CrearIngresoRequestDto
            {
                Monto = 2000,
                Descripcion = "Venta equipo",
                Fecha = fecha,//dateime.now
                Fuente = "Venta",
                Moneda = "BOB"
            });

            //Calculamos balance consolidado en BOB
            var resultado = await _ingresoService.CalcularBalanceConsolidadoAsync(
                new DateTime(2025, 12, 1), 
                new DateTime(2025, 12, 31),
                "BOB");

            //comprobamos q la moneda resultado sea BOB
            Assert.Equal("BOB", resultado.MonedaResultado);
            //el total 3000 + 2000 + (100 * 12.155) = 6215.5
            Assert.Equal(6215.5m, resultado.TotalConsolidado);
            //q se haya procesado 3 ingresos total
            Assert.Equal(3, resultado.TotalIngresosProcesados);
        }

        [Fact]
        public async Task CalcularBalanceConsolidado_EnDolares_SumaYConvierteCorrectamente()
        {
            _mockHexaRateService.Setup(s => s.ObtenerTipoCambioUsdBobAsync()).ReturnsAsync(12.155m);

            var fecha = new DateTime(2025, 12, 15);

            await _ingresoService.RegistrarIngresoAsync(new CrearIngresoRequestDto
            {
                Monto = 12155,
                Descripcion = "Venta local",
                Fecha = fecha,
                Fuente = "Ventas",
                Moneda = "BOB"
            });

            await _ingresoService.RegistrarIngresoAsync(new CrearIngresoRequestDto
            {
                Monto = 200,
                Descripcion = "Proyecto latam",
                Fecha = fecha,
                Fuente = "Freelance",
                Moneda = "USD"
            });

            var resultado = await _ingresoService.CalcularBalanceConsolidadoAsync(
                new DateTime(2025, 12, 1), 
                new DateTime(2025, 12, 31),
                "USD");

            //comprobar moneda result sea USD
            Assert.Equal("USD", resultado.MonedaResultado);
            //comprobar total (12155 / 12.155 = 1000) + 200 = 1200 USD
            Assert.Equal(1200m, resultado.TotalConsolidado);
            //comprobar que se hayan procesado 2 ingresos
            Assert.Equal(2, resultado.TotalIngresosProcesados);
        }
    }
}