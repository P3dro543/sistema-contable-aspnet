using Dapper;
using MySql.Data.MySqlClient;
using SistemaContable.Entities;
using System.Data;

namespace SistemaContable.Repository
{
    public class CuentaContableRepository
    {
        private readonly string _connectionString;

        public CuentaContableRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection Connection => new MySqlConnection(_connectionString);

        public async Task<IEnumerable<CuentaContable>> ObtenerPadresPosibles()
        {
            using var connection = Connection;
            var query = @"SELECT id_cuenta as IdCuenta, codigo as Codigo, nombre as Nombre, tipo as Tipo, 
                                 tipo_saldo as TipoSaldo, id_cuenta_padre as IdCuentaPadre, acepta_movimiento as AceptaMovimiento 
                          FROM cuentas_contables 
                          -- WHERE acepta_movimiento = 0 -- Se puede asociar a cualquier cuenta no transaccional
                          ORDER BY codigo";
            return await connection.QueryAsync<CuentaContable>(query);
        }

        public async Task<(IEnumerable<CuentaContable> Cuentas, int TotalRegistros)> ObtenerPaginados(int pagina, int registrosPorPagina)
        {
            using var connection = Connection;
            var offset = (pagina - 1) * registrosPorPagina;
            
            var totalQuery = "SELECT COUNT(*) FROM cuentas_contables";
            var totalRegistros = await connection.ExecuteScalarAsync<int>(totalQuery);

            var queryData = @"
                SELECT c.id_cuenta as IdCuenta, c.codigo as Codigo, c.nombre as Nombre, 
                       c.tipo as Tipo, c.tipo_saldo as TipoSaldo, c.id_cuenta_padre as IdCuentaPadre, 
                       c.acepta_movimiento as AceptaMovimiento 
                FROM cuentas_contables c
                ORDER BY c.codigo 
                LIMIT @Limit OFFSET @Offset";
            
            var cuentas = await connection.QueryAsync<CuentaContable>(queryData, new { Limit = registrosPorPagina, Offset = offset });

            return (cuentas, totalRegistros);
        }

        public async Task<CuentaContable?> ObtenerPorId(int id)
        {
            using var connection = Connection;
            var query = @"
                SELECT id_cuenta as IdCuenta, codigo as Codigo, nombre as Nombre, 
                       tipo as Tipo, tipo_saldo as TipoSaldo, id_cuenta_padre as IdCuentaPadre, 
                       acepta_movimiento as AceptaMovimiento 
                FROM cuentas_contables 
                WHERE id_cuenta = @Id";
            return await connection.QuerySingleOrDefaultAsync<CuentaContable>(query, new { Id = id });
        }

        public async Task<int> Insertar(CuentaContable cuenta)
        {
            using var connection = Connection;
            var query = @"
                INSERT INTO cuentas_contables (codigo, nombre, tipo, tipo_saldo, id_cuenta_padre, acepta_movimiento) 
                VALUES (@Codigo, @Nombre, @Tipo, @TipoSaldo, @IdCuentaPadre, @AceptaMovimiento)";
            return await connection.ExecuteAsync(query, cuenta);
        }

        public async Task<int> Actualizar(CuentaContable cuenta)
        {
            using var connection = Connection;
            var query = @"
                UPDATE cuentas_contables 
                SET codigo = @Codigo, nombre = @Nombre, tipo = @Tipo, 
                    tipo_saldo = @TipoSaldo, id_cuenta_padre = @IdCuentaPadre, acepta_movimiento = @AceptaMovimiento 
                WHERE id_cuenta = @IdCuenta";
            return await connection.ExecuteAsync(query, cuenta);
        }

        public async Task<int> Eliminar(int id)
        {
            using var connection = Connection;
            var query = "DELETE FROM cuentas_contables WHERE id_cuenta = @Id";
            return await connection.ExecuteAsync(query, new { Id = id });
        }
    }
}
