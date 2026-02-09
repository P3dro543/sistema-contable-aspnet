using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaContable.Services;

namespace SistemaContable.Web.Pages.PeriodoContable
{
    public class CreateModel : PageModel
    {
        private readonly PeriodoContableService _service;

        public CreateModel(PeriodoContableService service)
        {
            _service = service;
        }

        [BindProperty]
        public SistemaContable.Entities.PeriodoContable Periodo { get; set; } = new();

        public string Mensaje { get; set; } = "";

        public void OnGet()
        {
            Periodo.Anio = DateTime.Now.Year;
            Periodo.Mes = DateTime.Now.Month;
        }

        public async Task<IActionResult> OnPost()
        {
            var resultado = await _service.Insertar(Periodo, User.Identity?.Name ?? "admin");
            if (!resultado.exito)
            {
                Mensaje = resultado.mensaje;
                return Page();
            }
            TempData["Exito"] = "Nuevo periodo contable habilitado.";
            return RedirectToPage("Index");
        }
    }
}