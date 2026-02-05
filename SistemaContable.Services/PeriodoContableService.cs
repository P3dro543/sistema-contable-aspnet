using SistemaContable.Entities;
using SistemaContable.Repository;
using System.Text.Json;

namespace SistemaContable.Services
{
    public class PeriodoContableService
    {
        private readonly PeriodoContableRepository _repo;
        private readonly BitacoraRepository _bitacora;

        public PeriodoContableService(PeriodoContableRepository repo, BitacoraRepository bitacora)
        {
            _repo = repo;
            _bitacora = bitacora;
        }

        public async Task<IEnumerable<PeriodoContable>> ObtenerTodos(string filtro = "Todos")
            => await _repo.ObtenerTodos(filtro);

        public async Task<PeriodoContable?> ObtenerPorId(int id) => await _repo.ObtenerPorId(id);

        public async Task<(bool exito, string mensaje)> Insertar(PeriodoContable periodo, string usuario)
        {
            var validacion = await Validar(periodo);
            if (!validacion.esValido) return (false, validacion.mensaje);

            // Validar Estado numérico
            if (periodo.Estado == 0) periodo.Estado = 1; // Default a Abierto

            // Si es Cerrado (2), asignar usuario
            if (periodo.Estado == 2) periodo.UsuarioCierre = usuario;

            var id = await _repo.Insertar(periodo);
            periodo.IdPeriodo = id;

            // Cascada
            if (periodo.Estado == 2)
                await _repo.CerrarPeriodosAnteriores(periodo.Anio, periodo.Mes, usuario);

            await RegistrarBitacora(usuario, "Crear Periodo", JsonSerializer.Serialize(periodo));
            return (true, "Periodo creado exitosamente.");
        }

        public async Task<(bool exito, string mensaje)> Actualizar(PeriodoContable periodo, string usuario)
        {
            var anterior = await _repo.ObtenerPorId(periodo.IdPeriodo);
            if (anterior == null) return (false, "El periodo no existe.");

            var validacion = await Validar(periodo);
            if (!validacion.esValido) return (false, validacion.mensaje);

            // LOGICA ESTADOS (1=Abierto, 2=Cerrado)
            if (periodo.Estado == 2 && anterior.Estado == 1) // Cerrando
            {
                periodo.UsuarioCierre = usuario;
                await _repo.CerrarPeriodosAnteriores(periodo.Anio, periodo.Mes, usuario);
            }
            else if (periodo.Estado == 1 && anterior.Estado == 2) // Reabriendo
            {
                periodo.UsuarioCierre = null;
                await _repo.ReabrirPeriodosPosteriores(periodo.Anio, periodo.Mes);
            }

            await _repo.Actualizar(periodo);

            var detalle = new { anterior, nuevo = periodo };
            await RegistrarBitacora(usuario, "Actualizar Periodo", JsonSerializer.Serialize(detalle));

            return (true, "Periodo actualizado exitosamente.");
        }

        public async Task<(bool exito, string mensaje)> Eliminar(int id, string usuario)
        {
            var periodo = await _repo.ObtenerPorId(id);
            if (periodo == null) return (false, "El periodo no existe.");

            if (await _repo.TieneAsientosRelacionados(id))
                return (false, "No se puede eliminar un registro con datos relacionados.");

            await _repo.Eliminar(id);
            await RegistrarBitacora(usuario, "Eliminar Periodo", JsonSerializer.Serialize(periodo));
            return (true, "Periodo eliminado exitosamente.");
        }

        public async Task RegistrarConsulta(string usuario) => await RegistrarBitacora(usuario, "Consulta Periodos", "{}");

        private async Task<(bool esValido, string mensaje)> Validar(PeriodoContable periodo)
        {
            if (periodo.Anio < 2000 || periodo.Anio > 2100) return (false, "El año debe ser válido.");
            if (periodo.Mes < 1 || periodo.Mes > 12) return (false, "El mes debe ser entre 1 y 12.");

            if (await _repo.ExistePeriodo(periodo.Anio, periodo.Mes, periodo.IdPeriodo == 0 ? null : periodo.IdPeriodo))
                return (false, $"Ya existe un periodo registrado para {periodo.Mes}/{periodo.Anio}.");

            return (true, string.Empty);
        }

        private async Task RegistrarBitacora(string usuario, string accion, string detalle)
        {
            await _bitacora.Insertar(new Bitacora
            {
                Fecha = DateTime.Now,
                Usuario = usuario,
                Accion = accion,
                DetalleJson = detalle
            });
        }

        public async Task<(IEnumerable<PeriodoContable> items, int totalPaginas, int totalRegistros)> ObtenerListadoPaginado(int pagina, string filtro = "Todos")
        {
            const int TAMANO_PAGINA = 10;

            if (pagina < 1) pagina = 1;

            
            var resultado = await _repo.ObtenerPaginado(pagina, TAMANO_PAGINA, filtro);

            var totalPaginas = (int)Math.Ceiling((double)resultado.totalRegistros / TAMANO_PAGINA);

            return (resultado.items, totalPaginas, resultado.totalRegistros);
        }
    }
}