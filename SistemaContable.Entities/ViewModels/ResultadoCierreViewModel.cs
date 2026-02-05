namespace SistemaContable.Entities.ViewModels
{
    public class ResultadoCierreViewModel
    {
        public bool EsBalanceado { get; set; }
        public decimal TotalDebe { get; set; }
        public decimal TotalHaber { get; set; }
        public List<DetalleCierreCuenta> Cuentas { get; set; } = new();
        public string MensajeError { get; set; } = string.Empty;
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
    }

    public class DetalleCierreCuenta
    {
        public string Codigo { get; set; }
        public string Cuenta { get; set; }
        public decimal MovimientoDebe { get; set; }
        public decimal MovimientoHaber { get; set; }
    }
}