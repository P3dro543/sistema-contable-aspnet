using Dapper;
using MySql.Data.MySqlClient;
using SistemaContable.Entities;
using System.Data;

namespace SistemaContable.Repository
{
    public class RolRepository
    {
        private readonly string _connectionString;

        public RolRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection Connection => new MySqlConnection(_connectionString);

        public async Task<IEnumerable<Rol>> ObtenerTodos()
        {
            using var connection = Connection;
            var query = @"SELECT id_rol as IdRol, nombre as Nombre 
                         FROM roles 
                         ORDER BY nombre";
            var roles = await connection.QueryAsync<Rol>(query);

            foreach (var rol in roles)
            {
                rol.PantallasAsignadas = (await ObtenerPantallasDelRol(rol.IdRol)).ToList();
            }

            return roles;
        }

        public async Task<Rol?> ObtenerPorId(int id)
        {
            using var connection = Connection;
            var query = @"SELECT id_rol as IdRol, nombre as Nombre 
                         FROM roles 
                         WHERE id_rol = @Id";
            var rol = await connection.QueryFirstOrDefaultAsync<Rol>(query, new { Id = id });

            if (rol != null)
            {
                rol.PantallasAsignadas = (await ObtenerPantallasDelRol(rol.IdRol)).ToList();
            }

            return rol;
        }

        public async Task<IEnumerable<int>> ObtenerPantallasDelRol(int idRol)
        {
            using var connection = Connection;
            var query = "SELECT id_pantalla FROM rol_pantalla WHERE id_rol = @IdRol";
            return await connection.QueryAsync<int>(query, new { IdRol = idRol });
        }

        public async Task<int> Insertar(Rol rol)
        {
            using var connection = Connection;
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Insertar el rol
                var queryRol = @"INSERT INTO roles (nombre) VALUES (@Nombre);
                               SELECT LAST_INSERT_ID();";
                var idRol = await connection.ExecuteScalarAsync<int>(queryRol, new { rol.Nombre }, transaction);

                // Insertar las pantallas asignadas
                if (rol.PantallasAsignadas != null && rol.PantallasAsignadas.Any())
                {
                    var queryPantallas = "INSERT INTO rol_pantalla (id_rol, id_pantalla) VALUES (@IdRol, @IdPantalla)";
                    foreach (var idPantalla in rol.PantallasAsignadas)
                    {
                        await connection.ExecuteAsync(queryPantallas,
                            new { IdRol = idRol, IdPantalla = idPantalla }, transaction);
                    }
                }

                transaction.Commit();
                return idRol;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<int> Actualizar(Rol rol)
        {
            using var connection = Connection;
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Actualizar el rol
                var queryRol = "UPDATE roles SET nombre = @Nombre WHERE id_rol = @IdRol";
                await connection.ExecuteAsync(queryRol, rol, transaction);

                // Eliminar pantallas anteriores
                var queryEliminar = "DELETE FROM rol_pantalla WHERE id_rol = @IdRol";
                await connection.ExecuteAsync(queryEliminar, new { rol.IdRol }, transaction);

                // Insertar las nuevas pantallas asignadas
                if (rol.PantallasAsignadas != null && rol.PantallasAsignadas.Any())
                {
                    var queryPantallas = "INSERT INTO rol_pantalla (id_rol, id_pantalla) VALUES (@IdRol, @IdPantalla)";
                    foreach (var idPantalla in rol.PantallasAsignadas)
                    {
                        await connection.ExecuteAsync(queryPantallas,
                            new { rol.IdRol, IdPantalla = idPantalla }, transaction);
                    }
                }

                transaction.Commit();
                return rol.IdRol;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<int> Eliminar(int id)
        {
            using var connection = Connection;
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // PRIMERO: Eliminar las relaciones en rol_pantalla
                var queryEliminarPantallas = "DELETE FROM rol_pantalla WHERE id_rol = @Id";
                await connection.ExecuteAsync(queryEliminarPantallas, new { Id = id }, transaction);

                // SEGUNDO: Eliminar el rol
                var queryEliminarRol = "DELETE FROM roles WHERE id_rol = @Id";
                var result = await connection.ExecuteAsync(queryEliminarRol, new { Id = id }, transaction);

                transaction.Commit();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> TieneRelaciones(int id)
        {
            using var connection = Connection;
            var query = "SELECT COUNT(*) FROM usuario_rol WHERE id_rol = @Id";
            var count = await connection.ExecuteScalarAsync<int>(query, new { Id = id });
            return count > 0;
        }

        public async Task<bool> ExisteNombre(string nombre, int? idExcluir = null)
        {
            using var connection = Connection;
            var query = @"SELECT COUNT(*) FROM roles 
                         WHERE nombre = @Nombre AND (@IdExcluir IS NULL OR id_rol != @IdExcluir)";
            var count = await connection.ExecuteScalarAsync<int>(query, new { Nombre = nombre, IdExcluir = idExcluir });
            return count > 0;
        }
    }
}