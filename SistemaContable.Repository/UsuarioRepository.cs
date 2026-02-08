using Dapper;
using MySql.Data.MySqlClient;
using SistemaContable.Entities;
using System.Data;

namespace SistemaContable.Repository
{
    public class UsuarioRepository
    {
        private readonly string _connectionString;

        public UsuarioRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection Connection => new MySqlConnection(_connectionString);

        // Obtener todos los usuarios con sus roles
        public async Task<IEnumerable<Usuario>> ObtenerTodos()
        {
            using var connection = Connection;
            var query = @"SELECT id_usuario, username, nombre, apellido, correo, password, estado 
                         FROM usuarios 
                         ORDER BY nombre, apellido";

            var usuarios = await connection.QueryAsync<Usuario>(query);

            // Obtener roles para cada usuario
            foreach (var usuario in usuarios)
            {
                usuario.RolesAsignados = (await ObtenerRolesDelUsuario(usuario.id_usuario)).ToList();
            }

            return usuarios;
        }

        // Obtener usuario por ID
        public async Task<Usuario?> ObtenerPorId(int id)
        {
            using var connection = Connection;
            var query = @"SELECT id_usuario, username, nombre, apellido, correo, password, estado 
                         FROM usuarios 
                         WHERE id_usuario = @id";

            var usuario = await connection.QueryFirstOrDefaultAsync<Usuario>(query, new { id });

            if (usuario != null)
            {
                usuario.RolesAsignados = (await ObtenerRolesDelUsuario(usuario.id_usuario)).ToList();
            }

            return usuario;
        }

        // Obtener usuario por username
        public async Task<Usuario?> ObtenerPorUsername(string username)
        {
            using var connection = Connection;
            var query = @"SELECT id_usuario, username, nombre, apellido, correo, password, estado 
                         FROM usuarios 
                         WHERE username = @username";

            var usuario = await connection.QueryFirstOrDefaultAsync<Usuario>(query, new { username });

            if (usuario != null)
            {
                usuario.RolesAsignados = (await ObtenerRolesDelUsuario(usuario.id_usuario)).ToList();
            }

            return usuario;
        }

        // Obtener roles de un usuario
        public async Task<IEnumerable<int>> ObtenerRolesDelUsuario(int id_usuario)
        {
            using var connection = Connection;
            var query = "SELECT id_rol FROM usuario_rol WHERE id_usuario = @id_usuario";
            return await connection.QueryAsync<int>(query, new { id_usuario });
        }

        // Verificar si usuario tiene relaciones (roles asignados)
        public async Task<bool> TieneRelaciones(int id)
        {
            using var connection = Connection;
            var query = "SELECT COUNT(*) FROM usuario_rol WHERE id_usuario = @id";
            var count = await connection.ExecuteScalarAsync<int>(query, new { id });
            return count > 0;
        }

        // Verificar si username ya existe
        public async Task<bool> ExisteUsername(string username, int? idExcluir = null)
        {
            using var connection = Connection;
            var query = @"SELECT COUNT(*) FROM usuarios 
                         WHERE username = @username AND (@idExcluir IS NULL OR id_usuario != @idExcluir)";
            var count = await connection.ExecuteScalarAsync<int>(query, new { username, idExcluir });
            return count > 0;
        }

        // Crear usuario (con transacción)
        public async Task<int> Insertar(Usuario usuario)
        {
            using var connection = Connection;
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Insertar el usuario
                var queryUsuario = @"INSERT INTO usuarios (username, nombre, apellido, correo, password, estado) 
                                   VALUES (@username, @nombre, @apellido, @correo, @password, @estado);
                                   SELECT LAST_INSERT_ID();";

                var idUsuario = await connection.ExecuteScalarAsync<int>(queryUsuario, usuario, transaction);

                // Insertar los roles asignados
                if (usuario.RolesAsignados != null && usuario.RolesAsignados.Any())
                {
                    var queryRoles = "INSERT INTO usuario_rol (id_usuario, id_rol) VALUES (@id_usuario, @id_rol)";
                    foreach (var idRol in usuario.RolesAsignados)
                    {
                        await connection.ExecuteAsync(queryRoles,
                            new { id_usuario = idUsuario, id_rol = idRol }, transaction);
                    }
                }

                transaction.Commit();
                return idUsuario;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // Actualizar usuario (con transacción)
        public async Task<int> Actualizar(Usuario usuario)
        {
            using var connection = Connection;
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Actualizar el usuario (sin password)
                var queryUsuario = @"UPDATE usuarios 
                                   SET username = @username, nombre = @nombre, 
                                       apellido = @apellido, correo = @correo,
                                       estado = @estado
                                   WHERE id_usuario = @id_usuario";

                await connection.ExecuteAsync(queryUsuario, usuario, transaction);

                // Eliminar roles anteriores
                var queryEliminar = "DELETE FROM usuario_rol WHERE id_usuario = @id_usuario";
                await connection.ExecuteAsync(queryEliminar, new { usuario.id_usuario }, transaction);

                // Insertar los nuevos roles asignados
                if (usuario.RolesAsignados != null && usuario.RolesAsignados.Any())
                {
                    var queryRoles = "INSERT INTO usuario_rol (id_usuario, id_rol) VALUES (@id_usuario, @id_rol)";
                    foreach (var idRol in usuario.RolesAsignados)
                    {
                        await connection.ExecuteAsync(queryRoles,
                            new { usuario.id_usuario, id_rol = idRol }, transaction);
                    }
                }

                transaction.Commit();
                return usuario.id_usuario;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // Cambiar contraseña
        public async Task<bool> CambiarClave(int id_usuario, string nuevaClaveHash)
        {
            using var connection = Connection;
            var query = "UPDATE usuarios SET password = @password WHERE id_usuario = @id_usuario";
            var result = await connection.ExecuteAsync(query,
                new { id_usuario, password = nuevaClaveHash });
            return result > 0;
        }

        // Eliminar usuario (con validación de relaciones)
        public async Task<(bool éxito, string mensaje)> Eliminar(int id)
        {
            using var connection = Connection;
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Verificar si tiene relaciones
                var tieneRelaciones = await TieneRelaciones(id);
                if (tieneRelaciones)
                {
                    transaction.Rollback();
                    return (false, "No se puede eliminar un usuario con roles asignados.");
                }

                // Eliminar el usuario
                var queryEliminar = "DELETE FROM usuarios WHERE id_usuario = @id";
                var result = await connection.ExecuteAsync(queryEliminar, new { id }, transaction);

                transaction.Commit();
                return (result > 0, result > 0 ? "Usuario eliminado exitosamente." : "No se encontró el usuario.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return (false, $"Error al eliminar usuario: {ex.Message}");
            }
        }
    }
}