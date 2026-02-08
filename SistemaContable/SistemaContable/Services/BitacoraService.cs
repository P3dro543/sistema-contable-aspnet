// Services/BitacoraService.cs
using Dapper;
using SistemaContable.Models;
using System.Data;
using System.Text.Json;

namespace SistemaContable.Services
{
    public interface IBitacoraService
    {
        Task RegistrarCreacion(string usuario, object entidad, string entidadNombre);
        Task RegistrarActualizacion(string usuario, object entidadAnterior, object entidadActual, string entidadNombre);
        Task RegistrarEliminacion(string usuario, object entidad, string entidadNombre);
        Task RegistrarConsulta(string usuario, string entidadNombre);
    }

    public class BitacoraService : IBitacoraService
    {
        private readonly IDbConnection _db;

        public BitacoraService(IDbConnection db)
        {
            _db = db;
        }

        private async Task Registrar(string usuario, string accion, string detalleJson = null)
        {
            var sql = @"
                INSERT INTO bitacora (fecha, usuario, accion, detalle_json)
                VALUES (NOW(), @Usuario, @Accion, @DetalleJson)";

            await _db.ExecuteAsync(sql, new
            {
                Usuario = usuario,
                Accion = accion,
                DetalleJson = detalleJson
            });
        }

        public async Task RegistrarCreacion(string usuario, object entidad, string entidadNombre)
        {
            var json = JsonSerializer.Serialize(entidad);
            await Registrar(usuario, $"Crear {entidadNombre}", json);
        }

        public async Task RegistrarActualizacion(string usuario, object entidadAnterior, object entidadActual, string entidadNombre)
        {
            var detalle = new
            {
                Anterior = entidadAnterior,
                Actual = entidadActual
            };
            var json = JsonSerializer.Serialize(detalle);
            await Registrar(usuario, $"Actualizar {entidadNombre}", json);
        }

        public async Task RegistrarEliminacion(string usuario, object entidad, string entidadNombre)
        {
            var json = JsonSerializer.Serialize(entidad);
            await Registrar(usuario, $"Eliminar {entidadNombre}", json);
        }

        public async Task RegistrarConsulta(string usuario, string entidadNombre)
        {
            await Registrar(usuario, $"Consultar {entidadNombre}");
        }
    }
}