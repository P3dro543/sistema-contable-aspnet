using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaContable.Repository;

namespace SistemaContable.Web.Pages.CuentaContable
{
    public class EditModel : PageModel
    {
        private readonly CuentaContableRepository _repo;

        public EditModel(CuentaContableRepository repo)
        {
            _repo = repo;
        }

        [BindProperty]
        public SistemaContable.Entities.CuentaContable Cuenta { get; set; } = new();

        public IEnumerable<SistemaContable.Entities.CuentaContable> PadresPosibles { get; set; } = new List<SistemaContable.Entities.CuentaContable>();

        public string? MensajeError { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var cuentaDb = await _repo.ObtenerPorId(id);
            if (cuentaDb == null)
            {
                return RedirectToPage("Index");
            }

            Cuenta = cuentaDb;
            PadresPosibles = await _repo.ObtenerPadresPosibles();
            
            // Remover la propia cuenta de los padres posibles para evitar ciclos infinitos
            PadresPosibles = PadresPosibles.Where(p => p.IdCuenta != id).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                PadresPosibles = await _repo.ObtenerPadresPosibles();
                PadresPosibles = PadresPosibles.Where(p => p.IdCuenta != Cuenta.IdCuenta).ToList();
                return Page();
            }

            try
            {
                await _repo.Actualizar(Cuenta);
                TempData["Exito"] = "Cuenta contable actualizada correctamente.";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                MensajeError = "Error al actualizar la cuenta: " + ex.Message;
                PadresPosibles = await _repo.ObtenerPadresPosibles();
                PadresPosibles = PadresPosibles.Where(p => p.IdCuenta != Cuenta.IdCuenta).ToList();
                return Page();
            }
        }
    }
}
