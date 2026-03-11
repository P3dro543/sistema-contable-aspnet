using SistemaContable.Entities;
using SistemaContable.Repository;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;

namespace SistemaContable.Services
{
    public class UsuarioService
    {
        private readonly UsuarioRepository _usuarioRepository;
        private readonly RolRepository _rolRepository;
        private readonly BitacoraRepository _bitacoraRepository;

        public UsuarioService(
            UsuarioRepository usuarioRepository,
            RolRepository rolRepository,
            BitacoraRepository bitacoraRepository)
        {
            _usuarioRepository = usuarioRepository;
            _rolRepository = rolRepository;
            _bitacoraRepository = bitacoraRepository;
        }

        public async Task<IEnumerable<Usuario>> ObtenerTodos()
        {
            return await _usuarioRepository.ObtenerTodos();
        }

        public async Task<Usuario?> ObtenerPorId(int id)
        {
            return await _usuarioRepository.ObtenerPorId(id);
        }

        public async Task<(bool exito, string mensaje, int? id)> Insertar(Usuario usuario, string usuarioAccion)
        {
            // Validaciones
            var validacion = ValidarUsuario(usuario);
            if (!validacion.esValido)
            {
                return (false, validacion.mensaje, null);
            }

            // Verificar username duplicado
            if (await _usuarioRepository.ExisteUsername(usuario.username))
            {
                return (false, "Ya existe un usuario con ese nombre de usuario.", null);
            }

            // Hashear contraseña
            usuario.password = HashPassword(usuario.password);

            // Insertar usuario
            var idUsuario = await _usuarioRepository.Insertar(usuario);
            usuario.id_usuario = idUsuario;

            // Registrar en bitácora
            await RegistrarBitacora(usuarioAccion, "Crear usuario", JsonSerializer.Serialize(new
            {
                usuario.username,
                usuario.nombre,
                usuario.apellido,
                usuario.correo,
                usuario.estado,
                roles = usuario.RolesAsignados
            }));

            return (true, "Usuario creado exitosamente.", idUsuario);
        }

        public async Task<(bool exito, string mensaje)> Actualizar(Usuario usuario, string usuarioAccion)
        {
            // Obtener usuario anterior para bitácora
            var usuarioAnterior = await _usuarioRepository.ObtenerPorId(usuario.id_usuario);
            if (usuarioAnterior == null)
            {
                return (false, "El usuario no existe.");
            }

            // Validaciones
            var validacion = ValidarUsuario(usuario, true); // true = es actualización
            if (!validacion.esValido)
            {
                return (false, validacion.mensaje);
            }

            // Verificar username duplicado (excluyendo el actual)
            if (await _usuarioRepository.ExisteUsername(usuario.username, usuario.id_usuario))
            {
                return (false, "Ya existe un usuario con ese nombre de usuario.");
            }

            // Mantener la contraseña anterior (no se actualiza aquí)
            usuario.password = usuarioAnterior.password;

            await _usuarioRepository.Actualizar(usuario);

            // Registrar en bitácora
            var detalle = new
            {
                anterior = new
                {
                    usuarioAnterior.username,
                    usuarioAnterior.nombre,
                    usuarioAnterior.apellido,
                    usuarioAnterior.correo,
                    usuarioAnterior.estado,
                    roles = usuarioAnterior.RolesAsignados
                },
                nuevo = new
                {
                    usuario.username,
                    usuario.nombre,
                    usuario.apellido,
                    usuario.correo,
                    usuario.estado,
                    roles = usuario.RolesAsignados
                }
            };
            await RegistrarBitacora(usuarioAccion, "Actualizar usuario", JsonSerializer.Serialize(detalle));

            return (true, "Usuario actualizado exitosamente.");
        }

        public async Task<(bool exito, string mensaje)> Eliminar(int id, string usuarioAccion)
        {
            var usuario = await _usuarioRepository.ObtenerPorId(id);
            if (usuario == null)
            {
                return (false, "El usuario no existe.");
            }

            // Verificar si tiene relaciones (roles asignados)
            if (await _usuarioRepository.TieneRelaciones(id))
            {
                return (false, "No se puede eliminar un registro con datos relacionados.");
            }

            var resultado = await _usuarioRepository.Eliminar(id);
            if (!resultado.éxito)
            {
                return (false, resultado.mensaje);
            }

            // Registrar en bitácora
            await RegistrarBitacora(usuarioAccion, "Eliminar usuario", JsonSerializer.Serialize(new
            {
                usuario.username,
                usuario.nombre,
                usuario.apellido
            }));

            return (true, "Usuario eliminado exitosamente.");
        }

        public async Task<(bool exito, string mensaje)> CambiarClave(int id_usuario, string nuevaClave, string usuarioAccion)
        {
            var usuario = await _usuarioRepository.ObtenerPorId(id_usuario);
            if (usuario == null)
            {
                return (false, "El usuario no existe.");
            }

            // Validar formato de contraseña
            if (!Regex.IsMatch(nuevaClave, @"^[a-zA-Z][a-zA-Z0-9+\-*$.]+$"))
            {
                return (false, "La contraseña debe iniciar con letra y contener letras, números y símbolos (+-*$.)");
            }

            // Hashear y actualizar
            var claveHash = HashPassword(nuevaClave);
            var cambiado = await _usuarioRepository.CambiarClave(id_usuario, claveHash);

            if (!cambiado)
            {
                return (false, "No se pudo cambiar la contraseña.");
            }

            // Registrar en bitácora
            await RegistrarBitacora(usuarioAccion, "Cambio de clave usuario", JsonSerializer.Serialize(new
            {
                id_usuario,
                username = usuario.username
            }));

            return (true, "Contraseña cambiada exitosamente.");
        }

        // Método para generar contraseña automática (para ADM7 y ADM8)
        public string GenerarClaveAutomatica()
        {
            var random = new Random();
            var longitud = 12; // Longitud recomendada

            // Caracteres permitidos según especificaciones
            var letras = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var numeros = "0123456789";
            var simbolos = "+-*$.";
            var todosCaracteres = letras + numeros + simbolos;

            var clave = new StringBuilder();

            // 1. Primera letra (asegurar que empiece con letra - requisito)
            clave.Append(letras[random.Next(letras.Length)]);

            // 2. Resto de caracteres (mezclar)
            for (int i = 1; i < longitud; i++)
            {
                clave.Append(todosCaracteres[random.Next(todosCaracteres.Length)]);
            }

            return clave.ToString();
        }

        // Validaciones específicas para usuarios
        private (bool esValido, string mensaje) ValidarUsuario(Usuario usuario, bool esActualizacion = false)
        {
            // Validar username
            if (string.IsNullOrWhiteSpace(usuario.username))
            {
                return (false, "El nombre de usuario es requerido.");
            }

            if (usuario.username.Length > 50)
            {
                return (false, "El nombre de usuario no debe exceder 50 caracteres.");
            }

            // Validar nombre
            if (string.IsNullOrWhiteSpace(usuario.nombre))
            {
                return (false, "El nombre es requerido.");
            }

            if (usuario.nombre.Length > 50)
            {
                return (false, "El nombre no debe exceder 50 caracteres.");
            }

            if (!Regex.IsMatch(usuario.nombre, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                return (false, "El nombre solo debe contener letras y espacios.");
            }

            // Validar apellido
            if (string.IsNullOrWhiteSpace(usuario.apellido))
            {
                return (false, "El apellido es requerido.");
            }

            if (usuario.apellido.Length > 50)
            {
                return (false, "El apellido no debe exceder 50 caracteres.");
            }

            if (!Regex.IsMatch(usuario.apellido, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                return (false, "El apellido solo debe contener letras y espacios.");
            }

            // Validar correo
            if (string.IsNullOrWhiteSpace(usuario.correo))
            {
                return (false, "El correo electrónico es requerido.");
            }

            if (!Regex.IsMatch(usuario.correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                return (false, "El formato del correo electrónico no es válido.");
            }

            // Validar contraseña (solo para creación)
            if (!esActualizacion && string.IsNullOrWhiteSpace(usuario.password))
            {
                return (false, "La contraseña es requerida.");
            }

            // Validar estado
            if (usuario.estado != 0 && usuario.estado != 1)
            {
                return (false, "El estado debe ser 0 (Inactivo) o 1 (Activo).");
            }

            return (true, string.Empty);
        }

        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
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
            await RegistrarBitacora(usuario, "Consulta usuarios", "{}");
        }

        // Método para obtener todos los roles disponibles (para los dropdowns)
        public async Task<IEnumerable<Rol>> ObtenerRolesDisponibles()
        {
            return await _rolRepository.ObtenerTodos();
        }

        // Método para obtener usuario con roles disponibles (para edición)
        public async Task<Usuario?> ObtenerUsuarioConRoles(int id)
        {
            var usuario = await _usuarioRepository.ObtenerPorId(id);
            if (usuario != null)
            {
                // Obtener todos los roles para mostrar en el formulario
                var todosRoles = await _rolRepository.ObtenerTodos();

            }
            return usuario;
        }
    }
}