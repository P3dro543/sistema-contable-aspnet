using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaContable.Entities;
using SistemaContable.Services;
using EntidadesPantalla = SistemaContable.Entities.Pantalla;

namespace SistemaContable.Web.Pages.Pantalla
{
    public class EditModel : PageModel
    {
        private readonly PantallaService _service;

        public EditModel(PantallaService service)
        {
            _service = service;
        }

        [BindProperty]
        public EntidadesPantalla Pantalla { get; set; } = new EntidadesPantalla();

        public string Mensaje { get; set; } = "";

        public async Task<IActionResult> OnGet(int id)
        {
            var pantalla = await _service.ObtenerPorId(id);
            if (pantalla == null)
            {
                return RedirectToPage("Index");
            }

            Pantalla = pantalla;
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            var resultado = await _service.Actualizar(Pantalla, User.Identity?.Name ?? "admin");

            if (!resultado.exito)
            {
                Mensaje = resultado.mensaje;
                return Page();
            }

            return RedirectToPage("Index");
        }
    }
}
