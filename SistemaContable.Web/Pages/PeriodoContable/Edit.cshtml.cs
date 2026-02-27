using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaContable.Services;

namespace SistemaContable.Web.Pages.PeriodoContable
{
    public class EditModel : PageModel
    {
        private readonly PeriodoContableService _service;

        public EditModel(PeriodoContableService service)
        {
            _service = service;
        }

        
        [BindProperty(SupportsGet = true)]
        public int p { get; set; } // Página de origen

        [BindProperty(SupportsGet = true)]
        public string filtro { get; set; } // Filtro de origen
        [BindProperty] public SistemaContable.Entities.PeriodoContable Periodo { get; set; } = new();
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
            var resultado = await _service.Actualizar(Periodo, User.Identity?.Name ?? "admin");
            if (!resultado.exito)
            {
                Mensaje = resultado.mensaje;
                return Page();
            }
            return RedirectToPage("./Index", new { p = p, filtro = filtro });
        }
    }
}
