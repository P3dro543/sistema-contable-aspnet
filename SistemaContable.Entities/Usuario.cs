using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace SistemaContable.Entities
{
    public class Usuario
    {
        public int id_usuario { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(50, ErrorMessage = "Máximo 50 caracteres")]
        [Display(Name = "Nombre de usuario")]
        public string username { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(50, ErrorMessage = "Máximo 50 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "Solo letras y espacios")]
        [Display(Name = "Nombre")]
        public string nombre { get; set; }

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(50, ErrorMessage = "Máximo 50 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "Solo letras y espacios")]
        [Display(Name = "Apellido")]
        public string apellido { get; set; }

        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
        [Display(Name = "Correo electrónico")]
        public string correo { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [Display(Name = "Contraseña")]
        public string password { get; set; }

        [Required(ErrorMessage = "El estado es requerido")]
        [Display(Name = "Estado")]
        public int estado { get; set; } // 1 = Activo, 0 = Inactivo

        // Propiedades adicionales (no vienen de la BD)
        [Display(Name = "Roles asignados")]
        public List<int> RolesAsignados { get; set; } = new List<int>();

        public List<Rol> RolesDisponibles { get; set; } = new List<Rol>();

        public string NombreCompleto => $"{nombre} {apellido}";

        public bool EstaActivo => estado == 1;

        [Display(Name = "Estado")]
        public string EstadoTexto => estado == 1 ? "Activo" : "Inactivo";
    }

    // Para cambio de clave (ADM8)
    public class CambioClaveModel
    {
        public int id_usuario { get; set; }

        [Display(Name = "Usuario")]
        public string username { get; set; }

        [Display(Name = "Nombre completo")]
        public string nombre_completo { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mínimo 8 caracteres")]
        [RegularExpression(@"^[a-zA-Z][a-zA-Z0-9+\-*$.]+$",
            ErrorMessage = "Debe iniciar con letra y contener letras, números y símbolos (+-*$.)")]
        [Display(Name = "Nueva contraseña")]
        public string nueva_clave { get; set; }

        [Required(ErrorMessage = "Confirme la contraseña")]
        [Compare("nueva_clave", ErrorMessage = "Las contraseñas no coinciden")]
        [Display(Name = "Confirmar contraseña")]
        public string confirmar_clave { get; set; }
    }
}