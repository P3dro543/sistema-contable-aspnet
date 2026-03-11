using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Entities
{
    public class UsuarioP
    {
        public int IdUsuario { get; set; }
        public string nombre { get; set; } = string.Empty;
        public string apellido { get; set; } = string.Empty;
        public string correo { get; set; } = string.Empty;
        public string username { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public int idRol { get; set; }
        public string rol { get; set; } = string.Empty;
        public int estado { get; set; } 


    }
}
