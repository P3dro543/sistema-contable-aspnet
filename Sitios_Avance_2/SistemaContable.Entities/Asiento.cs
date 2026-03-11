namespace SistemaContable.Entities
{
    public class Asiento
    {
        public int Id { get; set; }
        public int Consecutivo { get; set; }
        public DateTime Fecha { get; set; }
        public string Referencia { get; set; }
        public int IdEstado { get; set; }
        public int IdPeriodo { get; set; }
        public List<DetalleAsiento> Detalles { get; set; } = new();

        // Propiedad calculada para compatibilidad con la vista si es necesario
        public string Codigo => ""; 
        public string Estado => IdEstado.ToString();
    }
}
