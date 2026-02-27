using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaContable.Entities;
using SistemaContable.Services;

namespace SistemaContable.Web.Pages.EstadoAsiento
{
    public class IndexModel : PageModel
    {
        private readonly EstadoAsientoService _service;

        public IndexModel(EstadoAsientoService service)
        {
            _service = service;
        }

        // Lista de datos que se mostrará en la tabla
        public IEnumerable<SistemaContable.Entities.EstadoAsiento> Estados { get; set; } = new List<SistemaContable.Entities.EstadoAsiento>();

        // Variables para controlar la paginación
        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; }

        // Este método se ejecuta al cargar la página
        // Recibe "p" desde la URL (ej: ?p=2)
        public async Task OnGet(int p = 1)
        {
            PaginaActual = p;

            // Llamamos al servicio nuevo que creamos para paginar
            var resultado = await _service.ObtenerListadoPaginado(p);

            Estados = resultado.items;
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

            return RedirectToPage(new { p = PaginaActual });
        }
    }
}