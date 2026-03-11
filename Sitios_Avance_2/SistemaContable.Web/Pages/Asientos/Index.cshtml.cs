using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaContable.Entities;
using SistemaContable.Services;

namespace SistemaContable.Pages.Asientos
{
    public class IndexModel : PageModel
    {
        private readonly AsientoService _service;


        public IndexModel(AsientoService service)
        {
            _service = service;
        }

        public List<Asiento> Asientos { get; set; } = new();

        public decimal TotalDebe { get; set; }
        public decimal TotalHaber { get; set; }

        public void OnGet()
        {
            Cargar();
        }

        private void Cargar()
        {
            Asientos = _service.Obtener();


            TotalDebe = Asientos
                .SelectMany(a => a.Detalles)
                .Where(d => d.TipoMovimiento == "Debe")
                .Sum(d => d.Monto);

            TotalHaber = Asientos
                .SelectMany(a => a.Detalles)
                .Where(d => d.TipoMovimiento == "Haber")
                .Sum(d => d.Monto);
        }
    }
}
