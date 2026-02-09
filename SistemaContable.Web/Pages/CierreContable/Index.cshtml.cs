using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaContable.Entities.ViewModels;
using SistemaContable.Services;

namespace SistemaContable.Web.Pages.CierreContable
{
    public class IndexModel : PageModel
    {
        private readonly PeriodoContableService _periodoService;
        private readonly CierreContableService _cierreService;

        public IndexModel(PeriodoContableService pService, CierreContableService cService)
        {
            _periodoService = pService;
            _cierreService = cService;
        }

       
        public List<SistemaContable.Entities.PeriodoContable> PeriodosAbiertos { get; set; } = new();

        public ResultadoCierreViewModel? Resultado { get; set; }

        [BindProperty]
        public int IdPeriodoSeleccionado { get; set; }

        public string Mensaje { get; set; } = "";
        public bool Exito { get; set; }

        public async Task OnGet(int? IdPeriodo, int p = 1)
        {
            await CargarPeriodos();
            if (IdPeriodo.HasValue)
            {
                IdPeriodoSeleccionado = IdPeriodo.Value;
                Resultado = await _cierreService.PrevisualizarCierre(IdPeriodoSeleccionado, p);
            }
        }

        public async Task OnPostPrevisualizar()
        {
            await CargarPeriodos();
            if (IdPeriodoSeleccionado != 0)
            {
                Resultado = await _cierreService.PrevisualizarCierre(IdPeriodoSeleccionado, 1);
            }
        }

        public async Task<IActionResult> OnPostConfirmar(int IdPeriodoConfirmado)
        {
            var res = await _cierreService.ConfirmarCierre(IdPeriodoConfirmado, User.Identity?.Name ?? "admin");

            if (res.exito)
            {
                
                TempData["Exito"] = "El cierre contable se ha realizado con éxito.";

              
                return RedirectToPage();
            }

            // Si falló, seguimos mostrando el error en la misma página
            Mensaje = res.mensaje;
            Exito = res.exito;
            await CargarPeriodos();
            return Page();
        }

        private async Task CargarPeriodos()
        {
            var todos = await _periodoService.ObtenerTodos("Abierto");
            // Filtramos solo los que tengan estado 1 (Abierto)
            PeriodosAbiertos = todos.Where(p => p.Estado == 1).OrderBy(p => p.Anio).ThenBy(p => p.Mes).ToList();
        }
    }
}