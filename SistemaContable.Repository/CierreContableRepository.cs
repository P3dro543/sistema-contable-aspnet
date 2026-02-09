using Dapper;
using MySql.Data.MySqlClient;
using SistemaContable.Entities.ViewModels;
using System.Data;

namespace SistemaContable.Repository
{
    public class CierreContableRepository
    {
        private readonly string _connectionString;

        public CierreContableRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection Connection => new MySqlConnection(_connectionString);

        // 1. Validar si hay periodos anteriores abiertos
        public async Task<bool> ExistenPeriodosAnterioresAbiertos(int anio, int mes)
        {
            using var connection = Connection;
            var sql = @"SELECT COUNT(*) FROM periodos_contables 
                        WHERE ((anio < @Anio) OR (anio = @Anio AND mes < @Mes)) 
                        AND estado = 1"; // 1 = Abierto
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Anio = anio, Mes = mes });
            return count > 0;
        }

        // 2. Obtener movimientos del periodo 
        public async Task<List<DetalleCierreCuenta>> ObtenerMovimientosDelPeriodo(int idPeriodo)
        {
            using var connection = Connection;

            // Esta consulta agrupa por cuenta y suma Debe vs Haber
            var sql = @"
                SELECT 
                    c.codigo as Codigo,
                    c.nombre as Cuenta,
                    SUM(CASE WHEN d.tipo_movimiento = 'Debe' THEN d.monto ELSE 0 END) as MovimientoDebe,
                    SUM(CASE WHEN d.tipo_movimiento = 'Haber' THEN d.monto ELSE 0 END) as MovimientoHaber
                FROM asiento_detalle d
                INNER JOIN asientos a ON d.id_asiento = a.id_asiento
                INNER JOIN cuentas_contables c ON d.id_cuenta = c.id_cuenta
                WHERE a.id_periodo = @IdPeriodo
                GROUP BY c.id_cuenta, c.codigo, c.nombre";

            var resultados = await connection.QueryAsync<DetalleCierreCuenta>(sql, new { IdPeriodo = idPeriodo });
            return resultados.ToList();
        }

        // 3. Cerrar el periodo (estado = 2)
        public async Task CerrarPeriodo(int idPeriodo, string usuario)
        {
            using var connection = Connection;
            var sql = @"UPDATE periodos_contables 
                        SET estado = 2, usuario_cierre = @Usuario 
                        WHERE id_periodo = @IdPeriodo";
            await connection.ExecuteAsync(sql, new { IdPeriodo = idPeriodo, Usuario = usuario });
        }

        public async Task<(List<DetalleCierreCuenta> items, int totalRegistros)> ObtenerMovimientosPaginados(int idPeriodo, int pagina, int registrosPorPagina)
        {
            using var connection = Connection;
            var offset = (pagina - 1) * registrosPorPagina;

            // Consulta para contar el total de cuentas con movimientos en ese periodo
            var sqlCount = @"
        SELECT COUNT(DISTINCT d.id_cuenta)
        FROM asiento_detalle d
        INNER JOIN asientos a ON d.id_asiento = a.id_asiento
        WHERE a.id_periodo = @IdPeriodo";

            // Consulta de datos con LIMIT y OFFSET
            var sqlData = @"
        SELECT 
            c.codigo as Codigo,
            c.nombre as Cuenta,
            SUM(CASE WHEN d.tipo_movimiento = 'Debe' THEN d.monto ELSE 0 END) as MovimientoDebe,
            SUM(CASE WHEN d.tipo_movimiento = 'Haber' THEN d.monto ELSE 0 END) as MovimientoHaber
        FROM asiento_detalle d
        INNER JOIN asientos a ON d.id_asiento = a.id_asiento
        INNER JOIN cuentas_contables c ON d.id_cuenta = c.id_cuenta
        WHERE a.id_periodo = @IdPeriodo
        GROUP BY c.id_cuenta, c.codigo, c.nombre
        ORDER BY c.codigo ASC
        LIMIT @Limit OFFSET @Offset";

            var total = await connection.ExecuteScalarAsync<int>(sqlCount, new { IdPeriodo = idPeriodo });
            var items = await connection.QueryAsync<DetalleCierreCuenta>(sqlData, new { IdPeriodo = idPeriodo, Limit = registrosPorPagina, Offset = offset });

            return (items.ToList(), total);
        }
    }
}