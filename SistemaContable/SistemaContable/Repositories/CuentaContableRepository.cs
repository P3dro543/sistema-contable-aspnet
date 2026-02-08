// Data/Repositories/CuentaContableRepository.cs
using Dapper;
using SistemaContable.Models;
using System.Data;

namespace SistemaContable.Data.Repositories
{
    public interface ICuentaContableRepository
    {
        Task<IEnumerable<CuentaContable>> GetAllAsync();
        Task<IEnumerable<CuentaContable>> GetActivasAsync(); // Nuevo: solo cuentas activas
        Task<CuentaContable?> GetByIdAsync(int id);
        Task<int> CreateAsync(CuentaContable cuenta);
        Task<bool> UpdateAsync(CuentaContable cuenta);
        Task<bool> DeleteAsync(int id);
        Task<bool> ToggleEstadoAsync(int id, bool nuevoEstado); // Nuevo: cambiar estado
        Task<bool> HasRelatedDataAsync(int id);
        Task<bool> HasMovimientosAsync(int id); // Nuevo: verificar movimientos
        Task<IEnumerable<CuentaContable>> GetCuentasPadreAsync();
        Task<IEnumerable<CuentaContable>> GetCuentasPadreActivasAsync(); // Nuevo: solo activas
        Task<int> CountHijasAsync(int idCuentaPadre);
        Task<bool> ExisteCodigoAsync(string codigo, int? idExcluir = null); // Nuevo: validar código único
    }

    public class CuentaContableRepository : ICuentaContableRepository
    {
        private readonly IDbConnection _db;

        public CuentaContableRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<CuentaContable>> GetAllAsync()
        {
            var sql = @"
                SELECT cc.*, 
                       cp.nombre as NombreCuentaPadre,
                       (SELECT COUNT(*) FROM cuentas_contables WHERE id_cuenta_padre = cc.id_cuenta) as NumeroHijas
                FROM cuentas_contables cc
                LEFT JOIN cuentas_contables cp ON cc.id_cuenta_padre = cp.id_cuenta
                ORDER BY cc.codigo";

            return await _db.QueryAsync<CuentaContable>(sql);
        }

        public async Task<IEnumerable<CuentaContable>> GetActivasAsync()
        {
            var sql = @"
                SELECT cc.*, 
                       cp.nombre as NombreCuentaPadre,
                       (SELECT COUNT(*) FROM cuentas_contables WHERE id_cuenta_padre = cc.id_cuenta) as NumeroHijas
                FROM cuentas_contables cc
                LEFT JOIN cuentas_contables cp ON cc.id_cuenta_padre = cp.id_cuenta
                WHERE cc.estado = 1
                ORDER BY cc.codigo";

            return await _db.QueryAsync<CuentaContable>(sql);
        }

        public async Task<CuentaContable?> GetByIdAsync(int id)
        {
            var sql = @"
                SELECT cc.*, 
                       cp.nombre as NombreCuentaPadre,
                       (SELECT COUNT(*) FROM cuentas_contables WHERE id_cuenta_padre = cc.id_cuenta) as NumeroHijas
                FROM cuentas_contables cc
                LEFT JOIN cuentas_contables cp ON cc.id_cuenta_padre = cp.id_cuenta
                WHERE cc.id_cuenta = @Id";

            return await _db.QueryFirstOrDefaultAsync<CuentaContable>(sql, new { Id = id });
        }

        public async Task<int> CreateAsync(CuentaContable cuenta)
        {
            // Validar que el código no exista
            if (await ExisteCodigoAsync(cuenta.Codigo))
            {
                throw new InvalidOperationException($"El código '{cuenta.Codigo}' ya existe en el sistema.");
            }

            var sql = @"
                INSERT INTO cuentas_contables 
                (codigo, nombre, tipo, tipo_saldo, id_cuenta_padre, acepta_movimiento, estado)
                VALUES 
                (@Codigo, @Nombre, @Tipo, @TipoSaldo, @IdCuentaPadre, @AceptaMovimiento, @Estado);
                SELECT LAST_INSERT_ID();";

            return await _db.ExecuteScalarAsync<int>(sql, cuenta);
        }

        public async Task<bool> UpdateAsync(CuentaContable cuenta)
        {
            // Validar que el código no exista (excluyendo el registro actual)
            if (await ExisteCodigoAsync(cuenta.Codigo, cuenta.IdCuenta))
            {
                throw new InvalidOperationException($"El código '{cuenta.Codigo}' ya existe en el sistema.");
            }

            // Si la cuenta tiene hijas, no puede aceptar movimiento
            if (cuenta.NumeroHijas > 0)
            {
                cuenta.AceptaMovimiento = false;
            }

            // Si tiene cuenta padre, no puede aceptar movimiento
            if (cuenta.IdCuentaPadre.HasValue)
            {
                cuenta.AceptaMovimiento = false;
            }

            var sql = @"
                UPDATE cuentas_contables 
                SET codigo = @Codigo,
                    nombre = @Nombre,
                    tipo = @Tipo,
                    tipo_saldo = @TipoSaldo,
                    id_cuenta_padre = @IdCuentaPadre,
                    acepta_movimiento = @AceptaMovimiento,
                    estado = @Estado
                WHERE id_cuenta = @IdCuenta";

            var rowsAffected = await _db.ExecuteAsync(sql, cuenta);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var sql = "DELETE FROM cuentas_contables WHERE id_cuenta = @Id";
            var rowsAffected = await _db.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<bool> ToggleEstadoAsync(int id, bool nuevoEstado)
        {
            var sql = "UPDATE cuentas_contables SET estado = @Estado WHERE id_cuenta = @Id";
            var rowsAffected = await _db.ExecuteAsync(sql, new
            {
                Id = id,
                Estado = nuevoEstado
            });
            return rowsAffected > 0;
        }

        public async Task<bool> HasRelatedDataAsync(int id)
        {
            // Verificar si la cuenta tiene asientos relacionados
            var sql = @"
                SELECT COUNT(*) 
                FROM asiento_detalle 
                WHERE id_cuenta = @Id";

            var count = await _db.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<bool> HasMovimientosAsync(int id)
        {
            // Verificar si la cuenta tiene movimientos (asientos aprobados)
            var sql = @"
                SELECT COUNT(*) 
                FROM asiento_detalle ad
                INNER JOIN asientos a ON ad.id_asiento = a.id_asiento
                INNER JOIN estados_asiento ea ON a.id_estado = ea.id_estado
                WHERE ad.id_cuenta = @Id 
                AND ea.nombre = 'Aprobado'";

            var count = await _db.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<IEnumerable<CuentaContable>> GetCuentasPadreAsync()
        {
            var sql = @"
                SELECT * FROM cuentas_contables 
                WHERE id_cuenta NOT IN (
                    SELECT DISTINCT id_cuenta_padre 
                    FROM cuentas_contables 
                    WHERE id_cuenta_padre IS NOT NULL
                )
                ORDER BY codigo";

            return await _db.QueryAsync<CuentaContable>(sql);
        }

        public async Task<IEnumerable<CuentaContable>> GetCuentasPadreActivasAsync()
        {
            var sql = @"
                SELECT * FROM cuentas_contables 
                WHERE estado = 1 
                AND id_cuenta NOT IN (
                    SELECT DISTINCT id_cuenta_padre 
                    FROM cuentas_contables 
                    WHERE id_cuenta_padre IS NOT NULL
                )
                ORDER BY codigo";

            return await _db.QueryAsync<CuentaContable>(sql);
        }

        public async Task<int> CountHijasAsync(int idCuentaPadre)
        {
            var sql = "SELECT COUNT(*) FROM cuentas_contables WHERE id_cuenta_padre = @Id";
            return await _db.ExecuteScalarAsync<int>(sql, new { Id = idCuentaPadre });
        }

        public async Task<bool> ExisteCodigoAsync(string codigo, int? idExcluir = null)
        {
            var sql = "SELECT COUNT(*) FROM cuentas_contables WHERE codigo = @Codigo";

            if (idExcluir.HasValue)
            {
                sql += " AND id_cuenta != @IdExcluir";
            }

            var count = await _db.ExecuteScalarAsync<int>(sql, new
            {
                Codigo = codigo,
                IdExcluir = idExcluir
            });

            return count > 0;
        }
    }
}