using SistemaContable.Entities;
using SistemaContable.Repository;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SistemaContable.Services
{
    public class EstadoAsientoService
    {
        private readonly EstadoAsientoRepository _repo;
        private readonly BitacoraRepository _bitacora;

        public EstadoAsientoService(EstadoAsientoRepository repo, BitacoraRepository bitacora)
        {
            _repo = repo;
            _bitacora = bitacora;
        }

        public async Task<IEnumerable<EstadoAsiento>> ObtenerTodos() => await _repo.ObtenerTodos();
        public async Task<EstadoAsiento?> ObtenerPorId(int id) => await _repo.ObtenerPorId(id);

        public async Task<(bool exito, string mensaje, int? id)> Insertar(EstadoAsiento estado, string usuario)
        {
            // Validaciones estrictas antes de llamar a la BD
            var validacion = Validar(estado);
            if (!validacion.esValido) return (false, validacion.mensaje, null);

            if (await _repo.ExisteNombre(estado.Nombre))
                return (false, "Ya existe un estado con ese nombre.", null);

            var id = await _repo.Insertar(estado);
            estado.IdEstado = id;

            await RegistrarBitacora(usuario, "Crear estado de asiento", JsonSerializer.Serialize(estado));
            return (true, "Estado creado exitosamente.", id);
        }

        public async Task<(bool exito, string mensaje)> Actualizar(EstadoAsiento estado, string usuario)
        {
            var anterior = await _repo.ObtenerPorId(estado.IdEstado);
            if (anterior == null) return (false, "El estado no existe.");

            // Validaciones estrictas
            var validacion = Validar(estado);
            if (!validacion.esValido) return (false, validacion.mensaje);

            if (await _repo.ExisteNombre(estado.Nombre, estado.IdEstado))
                return (false, "Ya existe otro estado con ese nombre.");

            await _repo.Actualizar(estado);

            var detalle = new { anterior, nuevo = estado };
            await RegistrarBitacora(usuario, "Actualizar estado de asiento", JsonSerializer.Serialize(detalle));

            return (true, "Estado actualizado exitosamente.");
        }

        public async Task<(bool exito, string mensaje)> Eliminar(int id, string usuario)
        {
            var estado = await _repo.ObtenerPorId(id);
            if (estado == null) return (false, "El estado no existe.");

            if (await _repo.TieneRelaciones(id))
                return (false, "No se puede eliminar un registro con datos relacionados.");

            await _repo.Eliminar(id);
            await RegistrarBitacora(usuario, "Eliminar estado de asiento", JsonSerializer.Serialize(estado));

            return (true, "Estado eliminado exitosamente.");
        }

        public async Task RegistrarConsulta(string usuario)
        {
            await RegistrarBitacora(usuario, "El usuario consulta estados de asiento", "{}");
        }

     
        private (bool esValido, string mensaje) Validar(EstadoAsiento estado)
        {
            
            if (string.IsNullOrWhiteSpace(estado.Nombre)) return (false, "El nombre es requerido.");

           
            if (estado.Nombre.Length > 40) return (false, "El nombre no debe exceder 40 caracteres.");

            if (!Regex.IsMatch(estado.Nombre, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$")) return (false, "El nombre solo debe contener letras.");

           
            if (string.IsNullOrWhiteSpace(estado.Descripcion)) return (false, "La descripción es requerida.");
            if (estado.Descripcion.Length > 200) return (false, "La descripción no debe exceder 200 caracteres.");

            
          
            if (!Regex.IsMatch(estado.Descripcion, @"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ\s]+$"))
                return (false, "La descripción solo debe tener letras, números y espacios.");

            return (true, string.Empty);
        }

        private async Task RegistrarBitacora(string usuario, string accion, string detalle)
        {
            // Usamos la entidad Bitacora tal cual la tienes
            await _bitacora.Insertar(new Bitacora
            {
                Fecha = DateTime.Now,
                Usuario = usuario,
                Accion = accion,
                DetalleJson = detalle
            });
        }

        
        public async Task<(IEnumerable<EstadoAsiento> items, int totalPaginas, int totalRegistros)> ObtenerListadoPaginado(int pagina)
        {
            const int TAMANO_PAGINA = 10; // Requerimiento: Máximo 10

            // Evitar páginas negativas
            if (pagina < 1) pagina = 1;

            var resultado = await _repo.ObtenerPaginado(pagina, TAMANO_PAGINA);

            // Calcular total de páginas (Ej: 25 registros / 10 = 2.5 -> Redondea a 3 páginas)
            var totalPaginas = (int)Math.Ceiling((double)resultado.totalRegistros / TAMANO_PAGINA);

            return (resultado.items, totalPaginas, resultado.totalRegistros);
        }
    }
}