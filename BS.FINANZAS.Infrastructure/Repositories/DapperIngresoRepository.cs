using System.Data;
using BS.FINANZAS.Domain.Entities;
using BS.FINANZAS.Domain.Interfaces;
using BS.FINANZAS.Infrastructure.Models;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace BS.FINANZAS.Infrastructure.Repositories
{
    public class DapperIngresoRepository : IIngresoRepository
    {
        private readonly string _connectionString;
        public DapperIngresoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("ConnectionString 'DefaultConnection' no encontrada.");
        }

        private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

        public async Task InicializarTablaAsync()
        {
            const string sql = @"
                CREATE TABLE IF NOT EXISTS Ingresos (
                    Id SERIAL PRIMARY KEY,
                    Monto NUMERIC(18, 2) NOT NULL,
                    Descripcion VARCHAR(250) NOT NULL,
                    Fecha TIMESTAMP NOT NULL,
                    Fuente VARCHAR(100) NOT NULL,
                    Moneda VARCHAR(10) NOT NULL
                );";

            using var connection = CreateConnection();

            await connection.ExecuteAsync(sql);
        }

        public async Task<Ingreso> AgregarAsync(Ingreso ingreso)
        {
            const string sql = @"
                INSERT INTO Ingresos (Monto, Descripcion, Fecha, Fuente, Moneda)
                VALUES (@Monto, @Descripcion, @Fecha, @Fuente, @MonedaStr)
                RETURNING Id;";

            using var connection = CreateConnection();
            var id = await connection.ExecuteScalarAsync<int>(sql, new
            {
                ingreso.Monto,
                ingreso.Descripcion,
                ingreso.Fecha,
                ingreso.Fuente,
                MonedaStr = ingreso.Moneda.ToString()
            });

            ingreso.Id = id;

            return ingreso;
        }

        public async Task<IEnumerable<Ingreso>> ObtenerIngresosAsync()
        {
            const string sql = @"SELECT Id, Monto, Descripcion, Fecha, Fuente, Moneda FROM Ingresos;";
            
            using var connection = CreateConnection();
            var result = await connection.QueryAsync<IngresoDbModel>(sql);
            return result.Select(MapToDomain);
        }

        public async Task<Ingreso?> ObtenerPorIdAsync(int id)
        {
            const string sql = @"SELECT Id, Monto, Descripcion, Fecha, Fuente, Moneda FROM Ingresos WHERE Id = @Id;";

            using var connection = CreateConnection();
            var dbModel = await connection.QueryFirstOrDefaultAsync<IngresoDbModel>(sql, new { Id = id });
            return dbModel == null ? null : MapToDomain(dbModel);
        }

        public async Task<IEnumerable<Ingreso>> ObtenerPorRangoFechasAsync(DateTime inicio, DateTime fin)
        {
            const string sql = @"
                SELECT Id, Monto, Descripcion, Fecha, Fuente, Moneda 
                FROM Ingresos 
                WHERE CAST(Fecha AS DATE) >= CAST(@Inicio AS DATE)
                AND CAST(Fecha AS DATE) <= CAST(@Fin AS DATE);";

            using var connection = CreateConnection();
            var result = await connection.QueryAsync<IngresoDbModel>(sql, new { Inicio = inicio, Fin = fin });
            return result.Select(MapToDomain);
        }

        public static Ingreso MapToDomain(IngresoDbModel db)
        {
            Enum.TryParse<Moneda>(db.Moneda, true, out var monedaParsed);
            return new Ingreso
            {
                Id = db.Id,
                Monto = db.Monto,
                Descripcion = db.Descripcion,
                Fecha = db.Fecha,
                Fuente = db.Fuente,
                Moneda = monedaParsed
            };
        }
    }
}
