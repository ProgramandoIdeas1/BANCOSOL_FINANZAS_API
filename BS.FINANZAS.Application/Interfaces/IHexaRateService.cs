namespace BS.FINANZAS.Application.Interfaces
{
    public interface IHexaRateService
    {
        Task<decimal> ObtenerTipoCambioUsdBobAsync();
    }
}