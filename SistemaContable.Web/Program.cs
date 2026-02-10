using SistemaContable.Middleware;
using SistemaContable.Repository;
using SistemaContable.Services;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(5);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "SistemaContable.Session";
});


// Add services to the container.
builder.Services.AddRazorPages();

var connectionString = builder.Configuration.GetConnectionString("MySqlConnection");

// Repositorios
builder.Services.AddScoped<AsientoRepository>(_ =>
    new AsientoRepository(connectionString));



builder.Services.AddScoped<PantallaRepository>(sp =>
    new PantallaRepository(connectionString));
builder.Services.AddScoped<RolRepository>(sp =>
    new RolRepository(connectionString));
builder.Services.AddScoped<BitacoraRepository>(sp =>
    new BitacoraRepository(connectionString));
// NUEVO GERALD 
builder.Services.AddScoped<EstadoAsientoRepository>(sp =>
    new EstadoAsientoRepository(connectionString));
builder.Services.AddScoped<PeriodoContableRepository>(sp =>
    new PeriodoContableRepository(connectionString));
builder.Services.AddScoped<CierreContableRepository>(sp => 
    new CierreContableRepository(connectionString));

builder.Services.AddScoped<SistemaContable.Repository.Auth>(sp =>
    new SistemaContable.Repository.Auth (connectionString));
builder.Services.AddScoped<UsuarioRepository>(sp =>
    new UsuarioRepository(connectionString));
// Servicios
builder.Services.AddScoped<PantallaService>();
builder.Services.AddScoped<RolService>();
// NUEVO GERALD 
builder.Services.AddScoped<EstadoAsientoService>();
builder.Services.AddScoped<PeriodoContableService>();
builder.Services.AddScoped<CierreContableService>();
builder.Services.AddScoped<AsientoService>();

builder.Services.AddScoped<SistemaContable.Services.Auth>();
builder.Services.AddScoped<UsuarioService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseSession();
app.UseMiddleware<ValidarSesionMiddleware>();// Agrega el middleware de validación de sesión 80

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapGet("/", (HttpContext context) =>
{
  
    context.Response.Redirect(context.Request.PathBase + "/Login/Login");

    return Task.CompletedTask;
});
app.MapRazorPages();
app.Run();  