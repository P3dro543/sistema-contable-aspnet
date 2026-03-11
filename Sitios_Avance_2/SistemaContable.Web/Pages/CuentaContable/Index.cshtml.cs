using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaContable.Repository;

namespace SistemaContable.Web.Pages.CuentaContable
{
    public class IndexModel : PageModel
    {
        private readonly CuentaContableRepository _repo;

        public IndexModel(CuentaContableRepository repo)
        {
            _repo = repo;
        }

        public IEnumerable<SistemaContable.Entities.CuentaContable> Cuentas { get; set; } = new List<SistemaContable.Entities.CuentaContable>();
        
        [TempData]
        public string? MensajeError { get; set; }

        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; }
        private const int PAGE_SIZE = 10;

        public async Task OnGetAsync(int p = 1)
        {
            PaginaActual = p < 1 ? 1 : p;
            
            var result = await _repo.ObtenerPaginados(PaginaActual, PAGE_SIZE);
            Cuentas = result.Cuentas;
            TotalPaginas = (int)Math.Ceiling(result.TotalRegistros / (double)PAGE_SIZE);
            if (TotalPaginas == 0) TotalPaginas = 1;
        }

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            try
            {
                await _repo.Eliminar(id);
                TempData["Exito"] = "Cuenta contable eliminada correctamente.";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("foreign key constraint fails") || ex.Message.ToLower().Contains("reference"))
                {
                    TempData["Error"] = "Validación RESTRICCIÓN BD: No se puede eliminar la cuenta porque tiene dependencias.";
                }
                else
                {
                    TempData["Error"] = "Error al intentar borrar la cuenta del sistema: " + ex.Message;
                }
                return RedirectToPage("Index");
            }
        }
    }
}
