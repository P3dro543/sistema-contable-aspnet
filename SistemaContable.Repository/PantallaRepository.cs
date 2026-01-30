using Dapper;
using MySql.Data.MySqlClient;
using SistemaContable.Entities;
using System.Data;

namespace SistemaContable.Repository
{
    public class PantallaRepository
    {
        private readonly string _connectionString;

        public PantallaRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection Connection => new MySqlConnection(_connectionString);

        public async Task<IEnumerable<Pantalla>> ObtenerTodas()
        {
            using var connection = Connection;
            var query = @"SELECT id_pantalla as IdPantalla, nombre as Nombre, 
                         descripcion as Descripcion, ruta as Ruta, estado as Estado 
                         FROM pantallas 
                         ORDER BY nombre";
            return await connection.QueryAsync<Pantalla>(query);
        }

        public async Task<Pantalla?> ObtenerPorId(int id)
        {
            using var connection = Connection;
            var query = @"SELECT id_pantalla as IdPantalla, nombre as Nombre, 
                         descripcion as Descripcion, ruta as Ruta, estado as Estado 
                         FROM pantallas 
                         WHERE id_pantalla = @Id";
            return await connection.QueryFirstOrDefaultAsync<Pantalla>(query, new { Id = id });
        }

        public async Task<int> Insertar(Pantalla pantalla)
        {
            using var connection = Connection;
            var query = @"INSERT INTO pantallas (nombre, descripcion, ruta, estado) 
                         VALUES (@Nombre, @Descripcion, @Ruta, @Estado);
                         SELECT LAST_INSERT_ID();";
            return await connection.ExecuteScalarAsync<int>(query, pantalla);
        }

        public async Task<int> Actualizar(Pantalla pantalla)
        {
            using var connection = Connection;
            var query = @"UPDATE pantallas 
                         SET nombre = @Nombre, descripcion = @Descripcion, 
                             ruta = @Ruta, estado = @Estado 
                         WHERE id_pantalla = @IdPantalla";
            return await connection.ExecuteAsync(query, pantalla);
        }

        public async Task<int> Eliminar(int id)
        {
            using var connection = Connection;
            var query = "DELETE FROM pantallas WHERE id_pantalla = @Id";
            return await connection.ExecuteAsync(query, new { Id = id });
        }

        public async Task<bool> TieneRelaciones(int id)
        {
            using var connection = Connection;
            var query = "SELECT COUNT(*) FROM rol_pantalla WHERE id_pantalla = @Id";
            var count = await connection.ExecuteScalarAsync<int>(query, new { Id = id });
            return count > 0;
        }

        public async Task<bool> ExisteNombre(string nombre, int? idExcluir = null)
        {
            using var connection = Connection;
            var query = @"SELECT COUNT(*) FROM pantallas 
                         WHERE nombre = @Nombre AND (@IdExcluir IS NULL OR id_pantalla != @IdExcluir)";
            var count = await connection.ExecuteScalarAsync<int>(query, new { Nombre = nombre, IdExcluir = idExcluir });
            return count > 0;
        }
    }
}