using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaContable.Services;
using SistemaContable.Entities;

namespace SistemaContable.Web.Pages.Usuarios
{
    public class CrearModel : PageModel
    {
        private readonly UsuarioService _usuarioService;
        private readonly RolService _rolService;

        [BindProperty]
        public Usuario Usuario { get; set; } = new Usuario();

        public List<Entities.Rol> RolesDisponibles { get; set; } = new List<Entities.Rol>();

        [BindProperty]
        public List<int> RolesSeleccionados { get; set; } = new List<int>();

        public string Mensaje { get; set; } = string.Empty;

        public CrearModel(UsuarioService usuarioService, RolService rolService)
        {
            _usuarioService = usuarioService;
            _rolService = rolService;
        }

        public async Task OnGetAsync()
        {
            await CargarRoles();
            // Generar contraseña automática al inicio
            Usuario.password = _usuarioService.GenerarClaveAutomatica();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // Asignar roles seleccionados
                Usuario.RolesAsignados = RolesSeleccionados;

                // Crear usuario
                var resultado = await _usuarioService.Insertar(Usuario, "admin");

                if (resultado.exito)
                {
                    TempData["MensajeExito"] = resultado.mensaje;
                    return RedirectToPage("Index");
                }
                else
                {
                    Mensaje = resultado.mensaje;
                    await CargarRoles();
                    return Page();
                }
            }
            catch (Exception ex)
            {
                Mensaje = $"Error: {ex.Message}";
                await CargarRoles();
                return Page();
            }
        }

        public IActionResult OnPostGenerarClave()
        {
            // Regenerar contraseña
            Usuario.password = _usuarioService.GenerarClaveAutomatica();
            return Page();
        }

        private async Task CargarRoles()
        {
            var roles = await _rolService.ObtenerTodos();
            RolesDisponibles = roles.ToList();
        }
    }
}