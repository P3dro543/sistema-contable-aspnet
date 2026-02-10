using System.Collections.Generic;

namespace SistemaContable.Entities
{
    public class CuentaContable
    {
        public int IdCuenta { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty; // Activo, Pasivo, Capital, Gasto, Ingreso
        public string TipoSaldo { get; set; } = string.Empty; // deudor, acreedor
        public int? IdCuentaPadre { get; set; }
        public bool AceptaMovimiento { get; set; } = false;
        public bool Estado { get; set; } = true; // activa/inactiva

        // Para relaciones (no mapeadas directamente)
        public CuentaContable? CuentaPadre { get; set; }
        public List<CuentaContable> CuentasHijas { get; set; } = new();
    }
}