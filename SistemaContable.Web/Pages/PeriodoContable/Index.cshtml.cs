using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaContable.Entities;
using SistemaContable.Services;

namespace SistemaContable.Web.Pages.PeriodoContable
{
    public class IndexModel : PageModel
    {
        private readonly PeriodoContableService _service;

        public IndexModel(PeriodoContableService service)
        {
            _service = service;
        }

        public IEnumerable<SistemaContable.Entities.PeriodoContable> Periodos { get; set; } = new List<SistemaContable.Entities.PeriodoContable>();

        // Propiedades para mantener el estado de la vista
        [BindProperty(SupportsGet = true)]
        public string Filtro { get; set; } = "Todos";

        [BindProperty(SupportsGet = true)]
        public int PaginaActual { get; set; } = 1;

        public int TotalPaginas { get; set; }

        public async Task OnGet(int p = 1, string filtro = "Todos")
        {
          
            PaginaActual = p;
            Filtro = filtro;

          
            if (PaginaActual < 1) PaginaActual = 1;

          
            var resultado = await _service.ObtenerListadoPaginado(PaginaActual, Filtro);

            Periodos = resultado.items;
            TotalPaginas = resultado.totalPaginas;
        }

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            var resultado = await _service.Eliminar(id, User.Identity?.Name ?? "admin");

            if (resultado.exito)
            {
                TempData["Exito"] = resultado.mensaje;
            }
            else
            {
                TempData["Error"] = resultado.mensaje;
            }

            return RedirectToPage(new { p = PaginaActual, filtro = Filtro });
        }
    }
}