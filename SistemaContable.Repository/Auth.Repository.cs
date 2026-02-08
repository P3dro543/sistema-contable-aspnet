using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using SistemaContable.Entities;
namespace SistemaContable.Repository
{
    public class Auth
    {
        private readonly string _connectionString;

        public Auth(string connectionString)
        {
            _connectionString = connectionString;
        }
        private IDbConnection Connection => new MySqlConnection(_connectionString);


        public async Task<UsuarioP?> ObtenerUsuarioPorUsername(String userName)
        {
            // consulta temporal para obtener usuario por username
            using var connection = Connection;
            var query = @"SELECT 
                u.id_usuario AS IdUsuario,
                u.nombre,
                u.apellido,
                u.correo,
                u.username,
                u.password,
                u.estado,
                r.id_rol AS idRol,
                r.nombre AS rol
            FROM usuarios u
            INNER JOIN usuario_rol ur 
                ON u.id_usuario = ur.id_usuario
            INNER JOIN roles r 
                ON ur.id_rol = r.id_rol
            WHERE u.username = @username;
            ";
            return await connection.QueryFirstOrDefaultAsync<UsuarioP>(query, new { username = userName });
        }
    }
}
