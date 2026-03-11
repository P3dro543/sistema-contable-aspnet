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
        public int IdCuenta { get; set; }
        public string Codigo { get; set; }
        public string Cuenta { get; set; }
        public string Naturaleza { get; set; } // Deudor o Acreedor
        public decimal SaldoInicial { get; set; }
        public decimal MovimientoDebe { get; set; }
        public decimal MovimientoHaber { get; set; }
        
        // Calculado en vuelo o traido de BD
        public decimal SaldoFinal => Naturaleza == "Deudor" 
            ? SaldoInicial + MovimientoDebe - MovimientoHaber 
            : SaldoInicial + MovimientoHaber - MovimientoDebe;
    }
}