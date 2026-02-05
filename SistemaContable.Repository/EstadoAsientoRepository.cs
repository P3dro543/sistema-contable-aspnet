using Dapper;
using MySql.Data.MySqlClient;
using SistemaContable.Entities;
using System.Data;

namespace SistemaContable.Repository
{
    public class EstadoAsientoRepository
    {
        private readonly string _connectionString;
 
        
        public EstadoAsientoRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection Connection => new MySqlConnection(_connectionString);

        public async Task<IEnumerable<EstadoAsiento>> ObtenerTodos()
        {
            using var connection = Connection;
            var query = @"SELECT id_estado as IdEstado, nombre as Nombre, 
                          descripcion as Descripcion 
                          FROM estados_asiento 
                          ORDER BY nombre";
            return await connection.QueryAsync<EstadoAsiento>(query);
        }

        public async Task<EstadoAsiento?> ObtenerPorId(int id)
        {
            using var connection = Connection;
            var query = @"SELECT id_estado as IdEstado, nombre as Nombre, 
                          descripcion as Descripcion 
                          FROM estados_asiento 
                          WHERE id_estado = @Id";
            return await connection.QueryFirstOrDefaultAsync<EstadoAsiento>(query, new { Id = id });
        }

        public async Task<int> Insertar(EstadoAsiento estado)
        {
            using var connection = Connection;
            var query = @"INSERT INTO estados_asiento (nombre, descripcion) 
                          VALUES (@Nombre, @Descripcion);
                          SELECT LAST_INSERT_ID();";
            return await connection.ExecuteScalarAsync<int>(query, estado);
        }

        public async Task<int> Actualizar(EstadoAsiento estado)
        {
            using var connection = Connection;
            var query = @"UPDATE estados_asiento 
                          SET nombre = @Nombre, descripcion = @Descripcion 
                          WHERE id_estado = @IdEstado";
            return await connection.ExecuteAsync(query, estado);
        }

        public async Task<int> Eliminar(int id)
        {
            using var connection = Connection;
            var query = "DELETE FROM estados_asiento WHERE id_estado = @Id";
            return await connection.ExecuteAsync(query, new { Id = id });
        }

        // Validación de Integridad Referencial 
        public async Task<bool> TieneRelaciones(int id)
        {
            using var connection = Connection;
            // Validamos contra la tabla 'asientos'
            var query = "SELECT COUNT(*) FROM asientos WHERE id_estado = @Id";
            var count = await connection.ExecuteScalarAsync<int>(query, new { Id = id });
            return count > 0;
        }

        public async Task<bool> ExisteNombre(string nombre, int? idExcluir = null)
        {
            using var connection = Connection;
            var query = @"SELECT COUNT(*) FROM estados_asiento 
                          WHERE nombre = @Nombre AND (@IdExcluir IS NULL OR id_estado != @IdExcluir)";
            var count = await connection.ExecuteScalarAsync<int>(query, new { Nombre = nombre, IdExcluir = idExcluir });
            return count > 0;
        }

        
        public async Task<(IEnumerable<EstadoAsiento> items, int totalRegistros)> ObtenerPaginado(int pagina, int registrosPorPagina)
        {
            using var connection = Connection;
            var offset = (pagina - 1) * registrosPorPagina;

            // Consulta 1: Trae solo los 10 registros que tocan
            var sqlData = @"SELECT id_estado as IdEstado, nombre, descripcion 
                    FROM estados_asiento 
                    ORDER BY id_estado DESC 
                    LIMIT @Limit OFFSET @Offset";

            // Consulta 2: Cuenta cuántos hay en total (para calcular los botones de abajo)
            var sqlCount = "SELECT COUNT(*) FROM estados_asiento";

            var items = await connection.QueryAsync<EstadoAsiento>(sqlData, new { Limit = registrosPorPagina, Offset = offset });
            var total = await connection.ExecuteScalarAsync<int>(sqlCount);

            return (items, total);
        }
    }
}