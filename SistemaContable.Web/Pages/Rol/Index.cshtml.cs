using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlX.XDevAPI;
using SistemaContable.Entities;
using SistemaContable.Services;

namespace SistemaContable.Web.Pages.Rol
{
    public class IndexModel : PageModel
    {
        private readonly RolService _service;

        public IndexModel(RolService service)
        {
            _service = service;
        }

        public IEnumerable<SistemaContable.Entities.Rol> Roles { get; set; }
            = new List<SistemaContable.Entities.Rol>();

        public string Mensaje { get; set; } = "";

        public async Task OnGet()
        {
            



            Roles = await _service.ObtenerTodos();
            HttpContext.Session.SetString("idRol", "2");
            await _service.RegistrarConsulta(User.Identity?.Name ?? "admin");
        }
    }
}
