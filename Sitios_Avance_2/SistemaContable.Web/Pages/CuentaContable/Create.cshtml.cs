using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaContable.Repository;

namespace SistemaContable.Web.Pages.CuentaContable
{
    public class CreateModel : PageModel
    {
        private readonly CuentaContableRepository _repo;

        public CreateModel(CuentaContableRepository repo)
        {
            _repo = repo;
        }

        [BindProperty]
        public SistemaContable.Entities.CuentaContable Cuenta { get; set; } = new();

        public IEnumerable<SistemaContable.Entities.CuentaContable> PadresPosibles { get; set; } = new List<SistemaContable.Entities.CuentaContable>();

        public string? MensajeError { get; set; }

        public async Task OnGetAsync()
        {
            PadresPosibles = await _repo.ObtenerPadresPosibles();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                PadresPosibles = await _repo.ObtenerPadresPosibles();
                return Page();
            }

            try
            {
                await _repo.Insertar(Cuenta);
                TempData["Exito"] = "Cuenta contable creada correctamente.";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                MensajeError = "Error al crear la cuenta: " + ex.Message;
                PadresPosibles = await _repo.ObtenerPadresPosibles();
                return Page();
            }
        }
    }
}
