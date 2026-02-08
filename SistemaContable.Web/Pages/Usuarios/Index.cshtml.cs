using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaContable.Services;
using SistemaContable.Entities;

namespace SistemaContable.Web.Pages.Usuarios
{
    public class IndexModel : PageModel
    {
        private readonly UsuarioService _usuarioService;
        private readonly ILogger<IndexModel> _logger;

        [TempData]
        public string MensajeExito { get; set; } = string.Empty;

        [TempData]
        public string MensajeError { get; set; } = string.Empty;

        public List<Usuario> Usuarios { get; set; } = new List<Usuario>();

        public IndexModel(UsuarioService usuarioService, ILogger<IndexModel> logger)
        {
            _usuarioService = usuarioService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            try
            {
                // Obtener todos los usuarios
                var usuarios = await _usuarioService.ObtenerTodos();
                Usuarios = usuarios.ToList();

                // Registrar consulta en bitácora
                await _usuarioService.RegistrarConsulta("admin");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios");
                MensajeError = "Error al cargar los usuarios.";
            }
        }

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            try
            {
                var resultado = await _usuarioService.Eliminar(id, "admin");

                if (resultado.exito)
                {
                    MensajeExito = resultado.mensaje;
                }
                else
                {
                    MensajeError = resultado.mensaje;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario");
                MensajeError = $"Error al eliminar usuario: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}