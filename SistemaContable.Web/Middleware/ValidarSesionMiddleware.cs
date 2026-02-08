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
            var path = context.Request.Path.Value?.ToLower();
            var nombre = context.Session.GetString("Nombre");

            // Permitir acceso a login y acceso denegado
            if (path.Contains("login") || path.Contains("accesodenegado"))
            {
                await _next(context);
                return;
            }

            if (string.IsNullOrEmpty(nombre))
            {
                context.Response.Redirect("/AccesoDenegado");
                return;
            }

            await _next(context);
        }
    }
}
