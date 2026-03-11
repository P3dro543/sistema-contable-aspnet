namespace SistemaContable.Entities
{
    public class DetalleAsiento
    {
        public int Id { get; set; }

        public int AsientoId { get; set; }

        public string Cuenta { get; set; }

        public string TipoMovimiento { get; set; }

        public decimal Monto { get; set; }

        public string Descripcion { get; set; }
    }
}

