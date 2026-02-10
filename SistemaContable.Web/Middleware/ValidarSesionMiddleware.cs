using SistemaContable.Entities;
using SistemaContable.Services;
using System.Threading.Tasks;



namespace SistemaContable.Middleware
{
    public class ValidarSesionMiddleware
    {
        private readonly RequestDelegate _next;

        public ValidarSesionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // 1️⃣ Ignorar archivos estáticos
            if (path.StartsWith("/css") ||
                path.StartsWith("/js") ||
                path.StartsWith("/images") ||
                path.StartsWith("/lib") ||
                path.StartsWith("/favicon")||
                path.StartsWith("/avatares"))
            {
                await _next(context);
                return;
            }

            // 2️⃣ Permitir páginas públicas
            if (path.Contains("login") || path.Contains("accesodenegado")|| path.Contains("expirada"))
            {
               
                await _next(context);
                return;
            }

            // 3️⃣ Permitir home
            if (path == "/index" || path == "/indexadmin"|| path=="/")
            {
                await _next(context);
                return;
            }

            var nombre = context.Session.GetString("Nombre");
            var idRolString = context.Session.GetString("idRol");

            // 4️⃣ Validar sesión
            if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(idRolString))
            {
                context.Response.Redirect("Expirada");
                return;
            }

            int idRol = Convert.ToInt32(idRolString);

            // 5️⃣ Resolver servicio scoped correctamente
            var pantallaService = context.RequestServices
                .GetRequiredService<PantallaService>();

            var pantallasxRol = await pantallaService.ObtenerPantallaPorRol(idRol);

            // 6️⃣ Validar acceso por ruta
            foreach (var pantalla in pantallasxRol)
            {
                var ruta = pantalla.Ruta.ToLower();

                if (path.Contains(ruta))
                {
                    await _next(context);
                    return;
                }
            }

            // 7️⃣ Bloquear acceso
            context.Response.Redirect("AccesoDenegado");
        }
    }


}
