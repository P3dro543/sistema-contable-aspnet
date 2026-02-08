// ViewModels/IndexCuentasViewModel.cs
using SistemaContable.Models;

namespace SistemaContable.ViewModels
{
    public class IndexCuentasViewModel
    {
        public IEnumerable<CuentaContable> Cuentas { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
    }
}