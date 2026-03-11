using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaContable.Services;
using EntidadesPantalla = SistemaContable.Entities.Pantalla;

namespace SistemaContable.Web.Pages.Rol
{
    public class DeleteModel : PageModel
    {
        private readonly RolService _rolService;
        private readonly PantallaService _pantallaService;

        public DeleteModel(RolService rolService, PantallaService pantallaService)
        {
            _rolService = rolService;
            _pantallaService = pantallaService;
        }

        [BindProperty]
        public SistemaContable.Entities.Rol Rol { get; set; } = new SistemaContable.Entities.Rol();

        public List<EntidadesPantalla> PantallasDelRol { get; set; } = new List<EntidadesPantalla>();

        public string Mensaje { get; set; } = "";

        public async Task<IActionResult> OnGet(int id)
        {
            var rol = await _rolService.ObtenerPorId(id);
            if (rol == null)
            {
                return RedirectToPage("Index");
            }

            Rol = rol;

            // Obtener las pantallas para mostrar sus nombres
            var todasPantallas = await _pantallaService.ObtenerTodas();
            PantallasDelRol = todasPantallas
                .Where(p => Rol.PantallasAsignadas.Contains(p.IdPantalla))
                .ToList();

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            var resultado = await _rolService.Eliminar(Rol.IdRol, User.Identity?.Name ?? "admin");

            if (!resultado.exito)
            {
                Mensaje = resultado.mensaje;

                // Recargar el rol y las pantallas para mostrar en caso de error
                var rol = await _rolService.ObtenerPorId(Rol.IdRol);
                if (rol != null)
                {
                    Rol = rol;
                    var todasPantallas = await _pantallaService.ObtenerTodas();
                    PantallasDelRol = todasPantallas
                        .Where(p => Rol.PantallasAsignadas.Contains(p.IdPantalla))
                        .ToList();
                }

                return Page();
            }

            return RedirectToPage("Index");
        }
    }
}