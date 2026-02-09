using Dapper;
using MySql.Data.MySqlClient;
using SistemaContable.Entities;
using System.Data;

namespace SistemaContable.Repository
{
    public class PeriodoContableRepository
    {
        private readonly string _connectionString;

        public PeriodoContableRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection Connection => new MySqlConnection(_connectionString);

        public async Task<IEnumerable<PeriodoContable>> ObtenerTodos(string filtroEstado = "Todos")
        {
            using var connection = Connection;
            var sql = @"SELECT id_periodo as IdPeriodo, anio as Anio, mes as Mes, 
                               estado as Estado, usuario_cierre as UsuarioCierre
                        FROM periodos_contables ";

            
            if (filtroEstado == "Abierto") sql += " WHERE estado = 1 ";
            else if (filtroEstado == "Cerrado") sql += " WHERE estado = 2 ";

            sql += " ORDER BY anio DESC, mes DESC";

            return await connection.QueryAsync<PeriodoContable>(sql);
        }

        public async Task<PeriodoContable?> ObtenerPorId(int id)
        {
            using var connection = Connection;
            var query = "SELECT id_periodo as IdPeriodo, anio as Anio, mes as Mes, estado as Estado, usuario_cierre as UsuarioCierre FROM periodos_contables WHERE id_periodo = @Id";
            return await connection.QueryFirstOrDefaultAsync<PeriodoContable>(query, new { Id = id });
        }

        public async Task<int> Insertar(PeriodoContable periodo)
        {
            using var connection = Connection;
            var query = @"INSERT INTO periodos_contables (anio, mes, estado, usuario_cierre) 
                          VALUES (@Anio, @Mes, @Estado, @UsuarioCierre);
                          SELECT LAST_INSERT_ID();";
            return await connection.ExecuteScalarAsync<int>(query, periodo);
        }

        public async Task Actualizar(PeriodoContable periodo)
        {
            using var connection = Connection;
            var query = @"UPDATE periodos_contables 
                          SET anio = @Anio, mes = @Mes, estado = @Estado, usuario_cierre = @UsuarioCierre
                          WHERE id_periodo = @IdPeriodo";
            await connection.ExecuteAsync(query, periodo);
        }

        public async Task Eliminar(int id)
        {
            using var connection = Connection;
            await connection.ExecuteAsync("DELETE FROM periodos_contables WHERE id_periodo = @Id", new { Id = id });
        }

        public async Task<bool> TieneAsientosRelacionados(int idPeriodo)
        {
            using var connection = Connection;
            try
            {
                var query = "SELECT COUNT(*) FROM asientos WHERE id_periodo = @Id";
                var count = await connection.ExecuteScalarAsync<int>(query, new { Id = idPeriodo });
                return count > 0;
            }
            catch { return false; }
        }

        // CASCADA CON INT (1=Abierto, 2=Cerrado)
        public async Task CerrarPeriodosAnteriores(int anio, int mes, string usuario)
        {
            using var connection = Connection;
            var query = @"UPDATE periodos_contables 
                          SET estado = 2, usuario_cierre = @Usuario
                          WHERE ((anio < @Anio) OR (anio = @Anio AND mes < @Mes)) 
                          AND estado = 1";
            await connection.ExecuteAsync(query, new { Anio = anio, Mes = mes, Usuario = usuario });
        }

        public async Task ReabrirPeriodosPosteriores(int anio, int mes)
        {
            using var connection = Connection;
            var query = @"UPDATE periodos_contables 
                           SET estado = 1, usuario_cierre = NULL
                           WHERE ((anio > @Anio) OR (anio = @Anio AND mes > @Mes)) 
                           AND estado = 2";
            await connection.ExecuteAsync(query, new { Anio = anio, Mes = mes });
        }

        public async Task<bool> ExistePeriodo(int anio, int mes, int? idExcluir = null)
        {
            using var connection = Connection;
            var query = @"SELECT COUNT(*) FROM periodos_contables 
                          WHERE anio = @Anio AND mes = @Mes 
                          AND (@IdExcluir IS NULL OR id_periodo != @IdExcluir)";
            var count = await connection.ExecuteScalarAsync<int>(query, new { Anio = anio, Mes = mes, IdExcluir = idExcluir });
            return count > 0;
        }

        
        public async Task<(IEnumerable<PeriodoContable> items, int totalRegistros)> ObtenerPaginado(int pagina, int registrosPorPagina, string filtroEstado = "Todos")
        {
            using var connection = Connection;
            var offset = (pagina - 1) * registrosPorPagina;

            // Base del SQL
            var sqlWhere = "";
            if (filtroEstado == "Abierto") sqlWhere = " WHERE estado = 1 ";
            else if (filtroEstado == "Cerrado") sqlWhere = " WHERE estado = 2 ";

            // Consulta de Datos (Limitada)
            var sqlData = $@"SELECT id_periodo as IdPeriodo, anio as Anio, mes as Mes, 
                            estado as Estado, usuario_cierre as UsuarioCierre
                     FROM periodos_contables 
                     {sqlWhere}
                     ORDER BY anio DESC, mes DESC 
                     LIMIT @Limit OFFSET @Offset";

            // Consulta de Conteo Total (Para saber cuántas páginas son)
            var sqlCount = $"SELECT COUNT(*) FROM periodos_contables {sqlWhere}";

            var items = await connection.QueryAsync<PeriodoContable>(sqlData, new { Limit = registrosPorPagina, Offset = offset });
            var total = await connection.ExecuteScalarAsync<int>(sqlCount);

            return (items, total);
        }
    }
}