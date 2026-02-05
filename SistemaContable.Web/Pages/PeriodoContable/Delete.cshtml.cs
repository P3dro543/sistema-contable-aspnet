using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaContable.Services;

namespace SistemaContable.Web.Pages.PeriodoContable
{
    public class DeleteModel : PageModel
    {
        private readonly PeriodoContableService _service;

        public DeleteModel(PeriodoContableService service)
        {
            _service = service;
        }

        [BindProperty]
        public SistemaContable.Entities.PeriodoContable Periodo { get; set; } = new();
        public string Mensaje { get; set; } = "";

        public async Task<IActionResult> OnGet(int id)
        {
            var p = await _service.ObtenerPorId(id);
            if (p == null) return RedirectToPage("Index");
            Periodo = p;
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            var resultado = await _service.Eliminar(Periodo.IdPeriodo, User.Identity?.Name ?? "admin");
            if (!resultado.exito)
            {
                Mensaje = resultado.mensaje; // Aquí saldrá el error de "datos relacionados"
                return Page();
            }
            return RedirectToPage("Index");
        }
    }
}