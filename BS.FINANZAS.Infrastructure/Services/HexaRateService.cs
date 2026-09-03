using System.Text.Json;
using BS.FINANZAS.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BS.FINANZAS.Infrastructure.Services
{
    public class HexaRateService : IHexaRateService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HexaRateService> _logger;
        private readonly string _apiUrl;
        private readonly decimal _defaultRate;
        public HexaRateService(HttpClient httpClient, ILogger<HexaRateService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiUrl = configuration["HexaRate:ApiUrl"] ?? "https://hexarate.paikama.co/api/rates/USD/BOB/latest";
            if (!decimal.TryParse(configuration["HexaRate:DefaultRate"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _defaultRate))
            {
                _defaultRate = 12.155m;//default
            }
        }

        public async Task<decimal> ObtenerTipoCambioUsdBobAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(jsonString);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("data", out var dataProp))
                    {
                        if (dataProp.TryGetProperty("mid", out var midProp))
                        {
                            return midProp.GetDecimal();
                        }
                        if (dataProp.TryGetProperty("rate", out var rateProp))
                        {
                            return rateProp.GetDecimal();
                        }
                    }

                    if (dataProp.TryGetProperty("mid", out var directMid))
                    {
                        return directMid.GetDecimal();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar HexaRate API. Usando tipo de cambio por defecto.");
                return _defaultRate;
            }
            return _defaultRate;
        }
    }
}
