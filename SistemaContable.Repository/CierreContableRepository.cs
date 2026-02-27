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

        public async Task<List<DetalleCierreCuenta>> ObtenerMovimientosDelPeriodo(int idPeriodo)
        {
            using var connection = Connection;

            var sql = @"
                SELECT 
                    c.id_cuenta as IdCuenta,
                    c.codigo as Codigo,
                    c.nombre as Cuenta,
                    c.tipo_saldo as Naturaleza,
                    COALESCE(s_ant.saldo_final, 0) as SaldoInicial,
                    SUM(CASE WHEN d.tipo_movimiento = 'Debe' THEN d.monto ELSE 0 END) as MovimientoDebe,
                    SUM(CASE WHEN d.tipo_movimiento = 'Haber' THEN d.monto ELSE 0 END) as MovimientoHaber
                FROM asiento_detalle d
                INNER JOIN asientos a ON d.id_asiento = a.id_asiento
                INNER JOIN cuentas_contables c ON d.id_cuenta = c.id_cuenta
                LEFT JOIN periodos_contables p_actual ON p_actual.id_periodo = a.id_periodo
                LEFT JOIN periodos_contables p_ant ON 
                    (p_actual.mes = 1 AND p_ant.mes = 12 AND p_ant.anio = p_actual.anio - 1)
                    OR (p_actual.mes != 1 AND p_ant.mes = p_actual.mes - 1 AND p_ant.anio = p_actual.anio)
                LEFT JOIN saldos_mensuales_cuenta s_ant ON s_ant.id_cuenta = c.id_cuenta AND s_ant.id_periodo = p_ant.id_periodo
                WHERE a.id_periodo = @IdPeriodo
                GROUP BY c.id_cuenta, c.codigo, c.nombre, c.tipo_saldo, s_ant.saldo_final";

            var resultados = await connection.QueryAsync<DetalleCierreCuenta>(sql, new { IdPeriodo = idPeriodo });
            return resultados.ToList();
        }

        public async Task CerrarPeriodo(int idPeriodo, string usuario, List<DetalleCierreCuenta> saldosFinales)
        {
            using var connection = Connection;
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var sqlCierre = @"UPDATE periodos_contables 
                            SET estado = 2, usuario_cierre = @Usuario 
                            WHERE id_periodo = @IdPeriodo";
                await connection.ExecuteAsync(sqlCierre, new { IdPeriodo = idPeriodo, Usuario = usuario }, transaction);

                var sqlInsertarSaldos = @"INSERT INTO saldos_mensuales_cuenta 
                                        (id_periodo, id_cuenta, saldo_inicial, debitos_mes, creditos_mes, saldo_final)
                                        VALUES (@IdPeriodo, @IdCuenta, @SaldoInicial, @MovDebe, @MovHaber, @SaldoFinal)";
                                        
                foreach (var cuenta in saldosFinales)
                {
                    await connection.ExecuteAsync(sqlInsertarSaldos, new 
                    {
                        IdPeriodo = idPeriodo,
                        IdCuenta = cuenta.IdCuenta,
                        SaldoInicial = cuenta.SaldoInicial,
                        MovDebe = cuenta.MovimientoDebe,
                        MovHaber = cuenta.MovimientoHaber,
                        SaldoFinal = cuenta.SaldoFinal
                    }, transaction);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
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