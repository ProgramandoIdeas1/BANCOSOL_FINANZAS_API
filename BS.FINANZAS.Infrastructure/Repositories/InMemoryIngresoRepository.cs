using System.Collections.Concurrent;
using BS.FINANZAS.Domain.Entities;
using BS.FINANZAS.Domain.Interfaces;

namespace BS.FINANZAS.Infrastructure.Repositories
{
    public class InMemoryIngresoRepository : IIngresoRepository
    {
        //dicc para almacenar ingresos por su id en memoria ram
        private readonly ConcurrentDictionary<int, Ingreso> _ingresos = new();
        //contador para autoincr el id
        private int _currentId = 0;

        //guardamos asicronamente un ingreso asignando un id incremental
        public Task<Ingreso> AgregarAsync(Ingreso ingreso)
        {
            ingreso.Id = Interlocked.Increment(ref _currentId);
            _ingresos[ingreso.Id] = ingreso;
            //return la tarea con la entidad agregada
            return Task.FromResult(ingreso);
        }

        //obtiene todos los ingresos en memoria ram
        public Task<IEnumerable<Ingreso>> ObtenerIngresosAsync()
        {
            return Task.FromResult(_ingresos.Values.AsEnumerable());
        }

        //obtiene un ingreso por su id o null si no existe en dict
        public Task<Ingreso?> ObtenerPorIdAsync(int id)
        {
            _ingresos.TryGetValue(id, out var ingreso);
            //return objeto o null
            return Task.FromResult(ingreso);
        }

        //filtra los ingresos enmemoria por rango de solo fechas
        public Task<IEnumerable<Ingreso>> ObtenerPorRangoFechasAsync(DateTime inicio, DateTime fin)
        {
            //filtro linq
            var resultado = _ingresos.Values
                .Where(i => i.Fecha.Date >= inicio.Date && i.Fecha.Date <= fin.Date);
            return Task.FromResult(resultado.AsEnumerable());
        }
    }
}