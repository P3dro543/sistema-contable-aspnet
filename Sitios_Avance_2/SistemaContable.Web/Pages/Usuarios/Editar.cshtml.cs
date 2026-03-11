using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaContable.Services;
using SistemaContable.Entities;

namespace SistemaContable.Web.Pages.Usuarios
{
    public class EditarModel : PageModel
    {
        private readonly UsuarioService _usuarioService;
        private readonly RolService _rolService;

        [BindProperty]
        public Usuario Usuario { get; set; } = new Usuario();

        public List<Entities.Rol> RolesDisponibles { get; set; } = new List<Entities.Rol>();

        [BindProperty]
        public List<int> RolesSeleccionados { get; set; } = new List<int>();

        public string Mensaje { get; set; } = string.Empty;

        public EditarModel(UsuarioService usuarioService, RolService rolService)
        {
            _usuarioService = usuarioService;
            _rolService = rolService;
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

                Usuario = usuario;
                RolesSeleccionados = usuario.RolesAsignados;
                await CargarRoles();

                return Page();
            }
            catch (Exception ex)
            {
                Mensaje = $"Error al cargar usuario: {ex.Message}";
                await CargarRoles();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // Asignar roles seleccionados
                Usuario.RolesAsignados = RolesSeleccionados;

                // Actualizar usuario (sin cambiar contraseña)
                var resultado = await _usuarioService.Actualizar(Usuario, "admin");

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

        private async Task CargarRoles()
        {
            var roles = await _rolService.ObtenerTodos();
            RolesDisponibles = roles.ToList();
        }
    }
}