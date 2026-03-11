using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaContable.Services;
using EntidadesPantalla = SistemaContable.Entities.Pantalla;

namespace SistemaContable.Web.Pages.Rol
{
    public class CreateModel : PageModel
    {
        private readonly RolService _rolService;
        private readonly PantallaService _pantallaService;

        public CreateModel(RolService rolService, PantallaService pantallaService)
        {
            _rolService = rolService;
            _pantallaService = pantallaService;
        }

        [BindProperty]
        public SistemaContable.Entities.Rol Rol { get; set; } = new SistemaContable.Entities.Rol();

        [BindProperty]
        public List<int> PantallasSeleccionadas { get; set; } = new List<int>();

        public IEnumerable<EntidadesPantalla> PantallasDisponibles { get; set; } = new List<EntidadesPantalla>();

        public string Mensaje { get; set; } = "";

        public async Task OnGet()
        {
            PantallasDisponibles = await _pantallaService.ObtenerTodas();
        }

        public async Task<IActionResult> OnPost()
        {
            // Cargar pantallas disponibles para mostrar en caso de error
            PantallasDisponibles = await _pantallaService.ObtenerTodas();

            // Asignar las pantallas seleccionadas al rol
            Rol.PantallasAsignadas = PantallasSeleccionadas;

            var resultado = await _rolService.Insertar(
                Rol,
                User.Identity?.Name ?? "admin"
            );

            if (!resultado.exito)
            {
                Mensaje = resultado.mensaje;
                return Page();
            }

            return RedirectToPage("Index");
        }
    }
}