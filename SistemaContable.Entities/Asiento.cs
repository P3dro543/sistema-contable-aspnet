namespace SistemaContable.Entities
{
    public class Asiento
    {
        public int Id { get; set; }

        public string Consecutivo { get; set; }

        public DateTime Fecha { get; set; }

        public string Codigo { get; set; }

        public string Referencia { get; set; }

        public string Estado { get; set; }

        public List<DetalleAsiento> Detalles { get; set; } = new();
    }
}
