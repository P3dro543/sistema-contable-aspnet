using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Entities
{
    public class Bitacora
    {
        public int IdBitacora { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;
        public string DetalleJson { get; set; } = string.Empty;
    }
}
