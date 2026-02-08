// Models/CuentaContable.cs
using System.ComponentModel.DataAnnotations;

namespace SistemaContable.Models
{
    public class CuentaContable
    {
        public int IdCuenta { get; set; }

        [Required(ErrorMessage = "El código es obligatorio")]
        [StringLength(20, ErrorMessage = "El código no puede exceder 20 caracteres")]
        public string Codigo { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "Solo letras, números y espacios")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El tipo es obligatorio")]
        public string Tipo { get; set; }

        [Required(ErrorMessage = "El tipo de saldo es obligatorio")]
        public string TipoSaldo { get; set; }

        public int? IdCuentaPadre { get; set; }

        [Required(ErrorMessage = "Debe indicar si acepta movimiento")]
        public bool AceptaMovimiento { get; set; } = false;

        [Required(ErrorMessage = "El estado es obligatorio")]
        public bool Estado { get; set; } = true; // true = Activa, false = Inactiva

        // Propiedades de navegación/derivadas
        public string? NombreCuentaPadre { get; set; }
        public int? NumeroHijas { get; set; }

        // Propiedades de solo lectura para la vista
        public string EstadoTexto => Estado ? "Activa" : "Inactiva";
        public string EstadoClase => Estado ? "bg-success" : "bg-secondary";

        public string TipoSaldoClase => TipoSaldo == "Deudor" ? "bg-danger" : "bg-success";

        public string AceptaMovimientoTexto => AceptaMovimiento ? "Sí" : "No";
        public string AceptaMovimientoClase => AceptaMovimiento ? "bg-success" : "bg-secondary";

        // Validación adicional
        public bool PuedeAceptarMovimiento => !IdCuentaPadre.HasValue && (NumeroHijas ?? 0) == 0;
    }
}