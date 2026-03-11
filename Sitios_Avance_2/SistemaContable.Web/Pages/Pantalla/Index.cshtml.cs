using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaContable.Services;
using EntidadesPantalla = SistemaContable.Entities.Pantalla;

namespace SistemaContable.Web.Pages.Pantalla
{
    public class IndexModel : PageModel
    {
        private readonly PantallaService _service;

        public IndexModel(PantallaService service)
        {
            _service = service;
        }

        public IEnumerable<EntidadesPantalla> Pantallas { get; set; }
            = new List<EntidadesPantalla>();

        public string Mensaje { get; set; } = "";

        public async Task OnGet()
        {
            Pantallas = await _service.ObtenerTodas();
            await _service.RegistrarConsulta(User.Identity?.Name ?? "admin");
        }
    }
}
