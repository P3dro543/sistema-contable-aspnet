using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaContable.Entities;
using SistemaContable.Services;

namespace SistemaContable.Web.Pages.Login
{
    public class LoginModel : PageModel
    {
        private readonly SistemaContable.Services.Auth _serviceAuth;

        public LoginModel(SistemaContable.Services.Auth service)
        {
            _serviceAuth = service;
        }

        [BindProperty]
        public UsuarioP NuevoUsuario { get; set; } = new UsuarioP();


        // Se ejecuta al cargar la página (GET)
        public void OnGet()
        {
        }

        // Se ejecuta al enviar el formulario (POST)
        public IActionResult OnPost()
        {
            UsuarioP usuarioxConsultar;
            try
            {
                //login correcto
                usuarioxConsultar = _serviceAuth.ObtenerUsuarioPorUsername(
                    NuevoUsuario.username,
                    NuevoUsuario.password
                ).GetAwaiter().GetResult();

                // guardar datos en session 
                
                
                HttpContext.Session.SetString("idRol",  (usuarioxConsultar.idRol).ToString());
                HttpContext.Session.SetString("Nombre", usuarioxConsultar.nombre + " " + usuarioxConsultar.apellido);
                HttpContext.Session.SetString("Rol", (usuarioxConsultar.rol).ToString());
                if (usuarioxConsultar.rol == "Administrador")
                {
                    return RedirectToPage("/IndexAdmin");
                }
                else {
                    return RedirectToPage("/IndexAdmin");
                }
                    //HttpContext.Session.SetString("", '');


                    //redireccionar a la página principal
                   
            }
            catch (Exception ex)
            {
                TempData["MensajeUsuario"] = ex.Message;
                TempData["Estado"] = "danger";
                return Page();
            }

        }
    }
}