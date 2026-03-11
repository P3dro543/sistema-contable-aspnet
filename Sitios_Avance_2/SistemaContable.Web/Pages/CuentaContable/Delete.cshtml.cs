using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaContable.Repository;

namespace SistemaContable.Web.Pages.CuentaContable
{
    public class DeleteModel : PageModel
    {
        private readonly CuentaContableRepository _repo;

        public DeleteModel(CuentaContableRepository repo)
        {
            _repo = repo;
        }

        [BindProperty]
        public SistemaContable.Entities.CuentaContable Cuenta { get; set; } = new();

        public string? MensajeError { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var cuentaDb = await _repo.ObtenerPorId(id);
            if (cuentaDb == null)
            {
                return RedirectToPage("Index");
            }

            Cuenta = cuentaDb;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                await _repo.Eliminar(Cuenta.IdCuenta);
                TempData["Exito"] = "Cuenta contable eliminada correctamente.";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                // Manejo de la excepcion de Foreign Key
                if (ex.Message.Contains("foreign key constraint fails") || ex.Message.ToLower().Contains("reference"))
                {
                    MensajeError = "Validación RESTRICCIÓN BD: No se puede eliminar la cuenta porque tiene transacciones de asientos, saldos mensuales o subcuentas que dependen de ella.";
                }
                else
                {
                    MensajeError = "Error al intentar borrar la cuenta del sistema: " + ex.Message;
                }
                
                // Refrescar entidad
                var cuentaDb = await _repo.ObtenerPorId(Cuenta.IdCuenta);
                if (cuentaDb != null)
                {
                    Cuenta = cuentaDb;
                }
                return Page();
            }
        }
    }
}
