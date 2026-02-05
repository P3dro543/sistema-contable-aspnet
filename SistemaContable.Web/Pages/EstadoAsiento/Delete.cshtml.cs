using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaContable.Services;

namespace SistemaContable.Web.Pages.EstadoAsiento
{
    public class DeleteModel : PageModel
    {
        private readonly EstadoAsientoService _service;

        public DeleteModel(EstadoAsientoService service)
        {
            _service = service;
        }

        [BindProperty]
        public SistemaContable.Entities.EstadoAsiento Estado { get; set; } = new();
        public string Mensaje { get; set; } = "";

        public async Task<IActionResult> OnGet(int id)
        {
            var estado = await _service.ObtenerPorId(id);
            if (estado == null) return RedirectToPage("Index");
            Estado = estado;
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            var resultado = await _service.Eliminar(Estado.IdEstado, User.Identity?.Name ?? "admin");
            if (!resultado.exito)
            {
                Mensaje = resultado.mensaje;
                return Page();
            }
            return RedirectToPage("Index");
        }
    }
}