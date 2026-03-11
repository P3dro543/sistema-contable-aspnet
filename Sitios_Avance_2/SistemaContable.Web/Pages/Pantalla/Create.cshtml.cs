using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaContable.Services;
using EntidadesPantalla = SistemaContable.Entities.Pantalla;

namespace SistemaContable.Web.Pages.Pantalla
{
    public class CreateModel : PageModel
    {
        private readonly PantallaService _service;

        public CreateModel(PantallaService service)
        {
            _service = service;
        }

        [BindProperty]
        public EntidadesPantalla Pantalla { get; set; } = new EntidadesPantalla();

        public string Mensaje { get; set; } = "";

        public async Task<IActionResult> OnPost()
        {
            var resultado = await _service.Insertar(
                Pantalla,
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
