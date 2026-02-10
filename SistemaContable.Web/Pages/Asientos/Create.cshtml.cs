using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaContable.Entities;
using SistemaContable.Services;

namespace SistemaContable.Pages.Asientos
{
    public class CreateModel : PageModel
    {
        private readonly AsientoService _service;

        // ?? DI: el framework inyecta AsientoService
        public CreateModel(AsientoService service)
        {
            _service = service;
        }

        [BindProperty]
        public Asiento Asiento { get; set; } = new();

        // ===== TOTALES =====
        public decimal TotalDebe { get; set; }
        public decimal TotalHaber { get; set; }

        public bool Balanceado => TotalDebe == TotalHaber;

        public void OnGet()
        {
            Asiento.Estado = "Borrador";
        }

        public IActionResult OnPost()
        {
            if (Asiento.Detalles == null || Asiento.Detalles.Count == 0)
            {
                ModelState.AddModelError("", "Debe agregar al menos una línea.");
                return Page();
            }

            TotalDebe = Asiento.Detalles
                .Where(x => x.TipoMovimiento == "Debe")
                .Sum(x => x.Monto);

            TotalHaber = Asiento.Detalles
                .Where(x => x.TipoMovimiento == "Haber")
                .Sum(x => x.Monto);

            if (TotalDebe != TotalHaber)
            {
                ModelState.AddModelError("",
                    "El asiento no está balanceado. Debe y Haber deben ser iguales.");
                return Page();
            }

            Asiento.Estado = "Pendiente";

            _service.Crear(Asiento);

            return RedirectToPage("Index");
        }
    }
}
