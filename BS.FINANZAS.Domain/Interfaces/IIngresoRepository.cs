using BS.FINANZAS.Domain.Entities;

namespace BS.FINANZAS.Domain.Interfaces
{
    public interface IIngresoRepository
    {
        Task<Ingreso> AgregarAsync(Ingreso ingreso);
        Task<IEnumerable<Ingreso>> ObtenerIngresosAsync();
        Task<Ingreso?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<Ingreso>> ObtenerPorRangoFechasAsync(DateTime inicio, DateTime fin);
    }
}
