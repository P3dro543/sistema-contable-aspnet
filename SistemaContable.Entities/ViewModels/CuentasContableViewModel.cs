using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemaContable.Entities.ViewModels
{
    public class CuentaContableViewModel
    {
        public int IdCuenta { get; set; }

        [Required(ErrorMessage = "El código es obligatorio")]
        [StringLength(20, ErrorMessage = "El código no puede exceder 20 caracteres")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$",
            ErrorMessage = "Solo se permiten letras, números y espacios")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo es obligatorio")]
        public string Tipo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de saldo es obligatorio")]
        public string TipoSaldo { get; set; } = string.Empty;

        public int? IdCuentaPadre { get; set; }
        public string? CuentaPadreNombre { get; set; }
        public bool AceptaMovimiento { get; set; }
        public bool Estado { get; set; } = true;
        public bool TieneHijas { get; set; }
        public bool TieneMovimientos { get; set; }
    }

    public class CuentaContableFormViewModel
    {
        public int IdCuenta { get; set; }

        [Required(ErrorMessage = "El código es obligatorio")]
        [StringLength(20, ErrorMessage = "El código no puede exceder 20 caracteres")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$",
            ErrorMessage = "Solo se permiten letras, números y espacios")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo es obligatorio")]
        public string Tipo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de saldo es obligatorio")]
        public string TipoSaldo { get; set; } = string.Empty;

        public int? IdCuentaPadre { get; set; }
        public bool AceptaMovimiento { get; set; }
        public bool Estado { get; set; } = true;

        // Para dropdowns
        public List<CuentaContable> CuentasPadreDisponibles { get; set; } = new();
        public List<string> TiposCuenta { get; set; } = new()
        {
            "Activo", "Pasivo", "Capital", "Gasto", "Ingreso"
        };
        public List<string> TiposSaldo { get; set; } = new()
        {
            "deudor", "acreedor"
        };
    }
}