using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaContable.Services;
using SistemaContable.Entities;

namespace SistemaContable.Web.Pages.EstadoAsiento
{
    public class CreateModel : PageModel
    {
        private readonly EstadoAsientoService _service;

        public CreateModel(EstadoAsientoService service)
        {
            _service = service;
        }

        [BindProperty]
        public SistemaContable.Entities.EstadoAsiento Estado { get; set; } = new();

        public string Mensaje { get; set; } = "";

        public void OnGet() { }

        public async Task<IActionResult> OnPost()
        {
            var resultado = await _service.Insertar(Estado, User.Identity?.Name ?? "admin");
            if (!resultado.exito)
            {
                Mensaje = resultado.mensaje;
                return Page();
            }
            TempData["Exito"] = "Estado creado correctamente.";
            return RedirectToPage("Index");
        }
    }
}