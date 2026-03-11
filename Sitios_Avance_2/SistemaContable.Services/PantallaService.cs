using SistemaContable.Entities;
using SistemaContable.Repository;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SistemaContable.Services
{
    public class PantallaService
    {
        private readonly PantallaRepository _pantallaRepository;
        private readonly BitacoraRepository _bitacoraRepository;

        public PantallaService(PantallaRepository pantallaRepository, BitacoraRepository bitacoraRepository)
        {
            _pantallaRepository = pantallaRepository;
            _bitacoraRepository = bitacoraRepository;
        }

        public async Task<IEnumerable<Pantalla>> ObtenerTodas()
        {
            return await _pantallaRepository.ObtenerTodas();
        }
        //Obtiene todas las pantallas, asignadas a un rol específico
        public async Task<IEnumerable<Pantalla>> ObtenerPantallaPorRol(int id)
        {
            return await _pantallaRepository.ObtenerPantallaPorRol(id);
        }

        public async Task<Pantalla?> ObtenerPorId(int id)
        {
            return await _pantallaRepository.ObtenerPorId(id);
        }

        public async Task<(bool exito, string mensaje, int? id)> Insertar(Pantalla pantalla, string usuario)
        {
            // Validaciones
            var validacion = ValidarPantalla(pantalla);
            if (!validacion.esValido)
            {
                return (false, validacion.mensaje, null);
            }

            // Verificar nombre duplicado
            if (await _pantallaRepository.ExisteNombre(pantalla.Nombre))
            {
                return (false, "Ya existe una pantalla con ese nombre.", null);
            }

            var id = await _pantallaRepository.Insertar(pantalla);
            pantalla.IdPantalla = id;

            // Registrar en bitácora
            await RegistrarBitacora(usuario, "Crear pantalla", JsonSerializer.Serialize(pantalla));

            return (true, "Pantalla creada exitosamente.", id);
        }

        public async Task<(bool exito, string mensaje)> Actualizar(Pantalla pantalla, string usuario)
        {
            // Obtener pantalla anterior para bitácora
            var pantallaAnterior = await _pantallaRepository.ObtenerPorId(pantalla.IdPantalla);
            if (pantallaAnterior == null)
            {
                return (false, "La pantalla no existe.");
            }

            // Validaciones
            var validacion = ValidarPantalla(pantalla);
            if (!validacion.esValido)
            {
                return (false, validacion.mensaje);
            }

            // Verificar nombre duplicado
            if (await _pantallaRepository.ExisteNombre(pantalla.Nombre, pantalla.IdPantalla))
            {
                return (false, "Ya existe una pantalla con ese nombre.");
            }

            await _pantallaRepository.Actualizar(pantalla);

            // Registrar en bitácora
            var detalle = new
            {
                anterior = pantallaAnterior,
                nuevo = pantalla
            };
            await RegistrarBitacora(usuario, "Actualizar pantalla", JsonSerializer.Serialize(detalle));

            return (true, "Pantalla actualizada exitosamente.");
        }

        public async Task<(bool exito, string mensaje)> Eliminar(int id, string usuario)
        {
            var pantalla = await _pantallaRepository.ObtenerPorId(id);
            if (pantalla == null)
            {
                return (false, "La pantalla no existe.");
            }

            // Verificar si tiene relaciones
            if (await _pantallaRepository.TieneRelaciones(id))
            {
                return (false, "No se puede eliminar un registro con datos relacionados.");
            }

            await _pantallaRepository.Eliminar(id);

            // Registrar en bitácora
            await RegistrarBitacora(usuario, "Eliminar pantalla", JsonSerializer.Serialize(pantalla));

            return (true, "Pantalla eliminada exitosamente.");
        }

        private (bool esValido, string mensaje) ValidarPantalla(Pantalla pantalla)
        {
            // Validar nombre
            if (string.IsNullOrWhiteSpace(pantalla.Nombre))
            {
                return (false, "El nombre es requerido.");
            }

            if (pantalla.Nombre.Length > 40)
            {
                return (false, "El nombre no debe exceder 40 caracteres.");
            }

            if (!Regex.IsMatch(pantalla.Nombre, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                return (false, "El nombre solo debe contener letras y espacios.");
            }

            // Validar descripción
            if (string.IsNullOrWhiteSpace(pantalla.Descripcion))
            {
                return (false, "La descripción es requerida.");
            }

            if (!Regex.IsMatch(pantalla.Descripcion, @"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                return (false, "La descripción solo debe contener letras, números y espacios.");
            }

            // Validar ruta
            if (string.IsNullOrWhiteSpace(pantalla.Ruta))
            {
                return (false, "La ruta es requerida.");
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
            await RegistrarBitacora(usuario, "El usuario consulta pantallas", "{}");
        }
    }
}