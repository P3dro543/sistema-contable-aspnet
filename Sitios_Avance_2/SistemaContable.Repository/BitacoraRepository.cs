using Dapper;
using MySql.Data.MySqlClient;
using SistemaContable.Entities;
using System.Data;

namespace SistemaContable.Repository
{
    public class BitacoraRepository
    {
        private readonly string _connectionString;

        public BitacoraRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection Connection => new MySqlConnection(_connectionString);

        public async Task<int> Insertar(Bitacora bitacora)
        {
            using var connection = Connection;
            var query = @"INSERT INTO bitacora (fecha, usuario, accion, detalle_json) 
                         VALUES (@Fecha, @Usuario, @Accion, @DetalleJson);
                         SELECT LAST_INSERT_ID();";
            return await connection.ExecuteScalarAsync<int>(query, bitacora);
        }
    }
}