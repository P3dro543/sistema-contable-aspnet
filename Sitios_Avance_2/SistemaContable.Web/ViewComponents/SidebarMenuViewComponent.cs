using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI;
using SistemaContable.Entities;
using SistemaContable.Services;
namespace SistemaContable.Web.ViewComponents
{
    public class SidebarMenuViewComponent : ViewComponent
    {
       
        private readonly PantallaService _servicePantalla;
        public SidebarMenuViewComponent(PantallaService service)
        {
            _servicePantalla = service;
        }

        public class SidebarMenuViewModel
        {
            public IEnumerable<Pantalla> Pantallas { get; set; }
            public string RutaLogo { get; set; }
            public string NombreUsuario { get; set; }
        }



        public async Task<IViewComponentResult> InvokeAsync()
        {
            
            if((HttpContext.Session.GetString("Nombre")== null) || HttpContext.Session.GetString("Nombre")=="")
            {
                HttpContext.Session.SetString("Nombre", "Invitado");
            }
            var vm = new SidebarMenuViewModel
            {
                NombreUsuario = HttpContext.Session.GetString("Nombre"),
                Pantallas = await ObtenerPantallasAsync(),
                
                RutaLogo = "~/images/logo-contador.png"

    };

            return View(vm);
        }

        private async Task<IEnumerable<Pantalla>> ObtenerPantallasAsync()
        {
            var idRol = Convert.ToInt32( HttpContext.Session.GetString("idRol"));

            
            return await _servicePantalla.ObtenerPantallaPorRol(idRol);
        }


    }
}
