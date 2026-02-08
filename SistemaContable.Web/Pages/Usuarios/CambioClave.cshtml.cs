using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaContable.Services;
using SistemaContable.Entities;

namespace SistemaContable.Web.Pages.Usuarios
{
    public class CambioClaveModel : PageModel
    {
        private readonly UsuarioService _usuarioService;

        [BindProperty]
        public CambioClaveModelVM Modelo { get; set; } = new CambioClaveModelVM();

        public string Mensaje { get; set; } = string.Empty;

        public CambioClaveModel(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var usuario = await _usuarioService.ObtenerPorId(id);
                if (usuario == null)
                {
                    return RedirectToPage("Index");
                }

                Modelo.id_usuario = usuario.id_usuario;
                Modelo.username = usuario.username;
                Modelo.nombre_completo = $"{usuario.nombre} {usuario.apellido}";

                // Generar contraseña automática inicial
                Modelo.nueva_clave = _usuarioService.GenerarClaveAutomatica();

                return Page();
            }
            catch (Exception ex)
            {
                Mensaje = $"Error al cargar usuario: {ex.Message}";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // Cambiar contraseña
                var resultado = await _usuarioService.CambiarClave(
                    Modelo.id_usuario,
                    Modelo.nueva_clave,
                    "admin");

                if (resultado.exito)
                {
                    TempData["MensajeExito"] = resultado.mensaje;
                    return RedirectToPage("Index");
                }
                else
                {
                    Mensaje = resultado.mensaje;
                    return Page();
                }
            }
            catch (Exception ex)
            {
                Mensaje = $"Error: {ex.Message}";
                return Page();
            }
        }

        public IActionResult OnPostAutogenerar()
        {
            // Generar nueva contraseña automática
            Modelo.nueva_clave = _usuarioService.GenerarClaveAutomatica();
            Modelo.confirmar_clave = Modelo.nueva_clave; // Auto-completar confirmación
            return Page();
        }
    }

    // ViewModel para esta página
    public class CambioClaveModelVM
    {
        public int id_usuario { get; set; }
        public string username { get; set; } = string.Empty;
        public string nombre_completo { get; set; } = string.Empty;
        public string nueva_clave { get; set; } = string.Empty;
        public string confirmar_clave { get; set; } = string.Empty;
    }
}