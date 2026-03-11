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
            var nombre = context.Session.GetString("Nombre");
            var idRolString = context.Session.GetString("idRol");
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
                if(path == "/indexadmin" && (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(idRolString)))
                {
                    context.Response.Redirect("/Expirada");
                    return;
                }
                await _next(context);
                return;
            }

            

            // 4️⃣ Validar sesión
            if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(idRolString))
            {
                context.Response.Redirect("/Expirada");
                return;
            }

            int idRol = Convert.ToInt32(idRolString);

            // 5.1️⃣ Administrador tiene acceso total (Seguridad extra)
            if (idRol == 1)
            {
                await _next(context);
                return;
            }

            // 5.2️⃣ Resolver servicio de pantallas
            var pantallaService = context.RequestServices.GetRequiredService<PantallaService>();
            var pantallasxRol = await pantallaService.ObtenerPantallaPorRol(idRol);

            // 6️⃣ Validar acceso por ruta (comparación normalizada)
            foreach (var pantalla in pantallasxRol)
            {
                var rutaDB = pantalla.Ruta.ToLower().Trim('/');
                var pathActual = path.Trim('/');

                if (pathActual.Contains(rutaDB) || rutaDB.Contains(pathActual))
                {
                    await _next(context);
                    return;
                }
            }

            // 7️⃣ Bloquear acceso (Redirección absoluta para evitar 404)
            context.Response.Redirect("/AccesoDenegado");
        }
    }


}
