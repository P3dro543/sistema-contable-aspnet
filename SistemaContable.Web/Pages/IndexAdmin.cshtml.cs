using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SistemaContable.Web.Pages
{
    public class IndexAdminModel : PageModel
    {
        public string NombreCompleto; 
        
        public void OnGet()
        {
            NombreCompleto = HttpContext.Session.GetString("Nombre") ?? "Usuario";
            ViewData["NombreCompleto"] = NombreCompleto;
        }
    }
}
