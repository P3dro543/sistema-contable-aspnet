using SistemaContable.Entities;
using SistemaContable.Repository;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SistemaContable.Services
{
    public class RolService
    {
        private readonly RolRepository _rolRepository;
        private readonly BitacoraRepository _bitacoraRepository;

        public RolService(RolRepository rolRepository, BitacoraRepository bitacoraRepository)
        {
            _rolRepository = rolRepository;
            _bitacoraRepository = bitacoraRepository;
        }

        public async Task<IEnumerable<Rol>> ObtenerTodos()
        {
            return await _rolRepository.ObtenerTodos();
        }

        public async Task<Rol?> ObtenerPorId(int id)
        {
            return await _rolRepository.ObtenerPorId(id);
        }

        public async Task<(bool exito, string mensaje, int? id)> Insertar(Rol rol, string usuario)
        {
            // Validaciones
            var validacion = ValidarRol(rol);
            if (!validacion.esValido)
            {
                return (false, validacion.mensaje, null);
            }

            // Verificar nombre duplicado
            if (await _rolRepository.ExisteNombre(rol.Nombre))
            {
                return (false, "Ya existe un rol con ese nombre.", null);
            }

            var id = await _rolRepository.Insertar(rol);
            rol.IdRol = id;

            // Registrar en bitácora
            await RegistrarBitacora(usuario, "Crear rol", JsonSerializer.Serialize(rol));

            return (true, "Rol creado exitosamente.", id);
        }

        public async Task<(bool exito, string mensaje)> Actualizar(Rol rol, string usuario)
        {
            // Obtener rol anterior para bitácora
            var rolAnterior = await _rolRepository.ObtenerPorId(rol.IdRol);
            if (rolAnterior == null)
            {
                return (false, "El rol no existe.");
            }

            // Validaciones
            var validacion = ValidarRol(rol);
            if (!validacion.esValido)
            {
                return (false, validacion.mensaje);
            }

            // Verificar nombre duplicado
            if (await _rolRepository.ExisteNombre(rol.Nombre, rol.IdRol))
            {
                return (false, "Ya existe un rol con ese nombre.");
            }

            await _rolRepository.Actualizar(rol);

            // Registrar en bitácora
            var detalle = new
            {
                anterior = rolAnterior,
                nuevo = rol
            };
            await RegistrarBitacora(usuario, "Actualizar rol", JsonSerializer.Serialize(detalle));

            return (true, "Rol actualizado exitosamente.");
        }

        public async Task<(bool exito, string mensaje)> Eliminar(int id, string usuario)
        {
            var rol = await _rolRepository.ObtenerPorId(id);
            if (rol == null)
            {
                return (false, "El rol no existe.");
            }

            // Verificar si tiene relaciones
            if (await _rolRepository.TieneRelaciones(id))
            {
                return (false, "No se puede eliminar un registro con datos relacionados.");
            }

            await _rolRepository.Eliminar(id);

            // Registrar en bitácora
            await RegistrarBitacora(usuario, "Eliminar rol", JsonSerializer.Serialize(rol));

            return (true, "Rol eliminado exitosamente.");
        }

        private (bool esValido, string mensaje) ValidarRol(Rol rol)
        {
            // Validar nombre
            if (string.IsNullOrWhiteSpace(rol.Nombre))
            {
                return (false, "El nombre es requerido.");
            }

            if (rol.Nombre.Length > 40)
            {
                return (false, "El nombre no debe exceder 40 caracteres.");
            }

            if (!Regex.IsMatch(rol.Nombre, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                return (false, "El nombre solo debe contener letras y espacios.");
            }

            return (true, string.Empty);
        }

        private async Task RegistrarBitacora(string usuario, string accion, string detalleJson)
        {
            var bitacora = new Bitacora
            {
                Fecha = DateTime.Now,
                Usuario = usuario,
                Accion = accion,
                DetalleJson = detalleJson
            };
            await _bitacoraRepository.Insertar(bitacora);
        }

        public async Task RegistrarConsulta(string usuario)
        {
            await RegistrarBitacora(usuario, "El usuario consulta roles", "{}");
        }
    }
}