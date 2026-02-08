// Tests/CuentasContablesTests.cs
using Xunit;
using SistemaContable.Models;
using SistemaContable.Data.Repositories;
using System.Data;
using MySql.Data.MySqlClient;

namespace SistemaContable.Tests
{
    public class CuentasContablesTests
    {
        private readonly string _connectionString = "tu_connection_string";

        [Xunit.Fact]
        public async Task CrearCuentaContable_Valida_CamposRequeridos()
        {
            // Arrange
            var cuenta = new CuentaContable
            {
                Codigo = "1101",
                Nombre = "Caja General",
                Tipo = "Activo",
                TipoSaldo = "Deudor",
                AceptaMovimiento = true
            };

            // Act & Assert
            using (var connection = new MySqlConnection(_connectionString))
            {
                var repo = new CuentaContableRepository(connection);
                var id = await repo.CreateAsync(cuenta);

                Xunit.Assert.True(id > 0);

                // Verificar que se creó
                var cuentaCreada = await repo.GetByIdAsync(id);
                Xunit.Assert.NotNull(cuentaCreada);
                Xunit.Assert.Equal("1101", cuentaCreada.Codigo);

                // Cleanup
                await repo.DeleteAsync(id);
            }
        }

        [Xunit.Fact]
        public async Task NoPermitirEliminarCuenta_ConDatosRelacionados()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var repo = new CuentaContableRepository(connection);

                // Crear cuenta y relacionarla con un asiento
                // (Aquí necesitarías datos de prueba)

                var tieneRelaciones = await repo.HasRelatedDataAsync(1);

                // Assert
                Xunit.Assert.True(tieneRelaciones); // Debería tener relaciones
            }
        }

        [Xunit.Theory]
        [Xunit.InlineData("Activo", "Deudor", true)]
        [Xunit.InlineData("Pasivo", "Acreedor", true)]
        [Xunit.InlineData("", "Deudor", false)] // Tipo vacío
        [Xunit.InlineData("Activo", "", false)] // TipoSaldo vacío
        public async Task ValidarCombinacionesTipoTipoSaldo(string tipo, string tipoSaldo, bool esperadoValido)
        {
            var cuenta = new CuentaContable
            {
                Codigo = "9999",
                Nombre = "Cuenta Test",
                Tipo = tipo,
                TipoSaldo = tipoSaldo,
                AceptaMovimiento = true
            };

            using (var connection = new MySqlConnection(_connectionString))
            {
                var repo = new CuentaContableRepository(connection);

                if (esperadoValido)
                {
                    var id = await repo.CreateAsync(cuenta);
                    Xunit.Assert.True(id > 0);
                    await repo.DeleteAsync(id);
                }
                else
                {
                    await Xunit.Assert.ThrowsAnyAsync<System.Exception>(() => repo.CreateAsync(cuenta));
                }
            }
        }
    }
}