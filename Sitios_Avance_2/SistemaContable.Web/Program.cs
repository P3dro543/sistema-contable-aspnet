using SistemaContable.Middleware;
using SistemaContable.Repository;
using SistemaContable.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuracion de Sesion (Pr2)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "SistemaContable.Session";
});

// Add services to the container.
builder.Services.AddRazorPages();

var connectionString = builder.Configuration.GetConnectionString("MySqlConnection");

// Repositorios

builder.Services.AddScoped<PantallaRepository>(sp => new PantallaRepository(connectionString));
builder.Services.AddScoped<RolRepository>(sp => new RolRepository(connectionString));
builder.Services.AddScoped<BitacoraRepository>(sp => new BitacoraRepository(connectionString));
builder.Services.AddScoped<CuentaContableRepository>(sp => new CuentaContableRepository(connectionString)); // GERAL
builder.Services.AddScoped<EstadoAsientoRepository>(sp => new EstadoAsientoRepository(connectionString));
builder.Services.AddScoped<PeriodoContableRepository>(sp => new PeriodoContableRepository(connectionString));
builder.Services.AddScoped<CierreContableRepository>(sp => new CierreContableRepository(connectionString));
builder.Services.AddScoped<UsuarioRepository>(sp => new UsuarioRepository(connectionString));
builder.Services.AddScoped<SistemaContable.Repository.Auth>(sp => new SistemaContable.Repository.Auth(connectionString));

// Servicios
builder.Services.AddScoped<PantallaService>();
builder.Services.AddScoped<RolService>();
builder.Services.AddScoped<EstadoAsientoService>();
builder.Services.AddScoped<PeriodoContableService>();
builder.Services.AddScoped<CierreContableService>();
builder.Services.AddScoped<AsientoService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<SistemaContable.Services.Auth>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseMiddleware<ValidarSesionMiddleware>(); // Seguridad PR2
app.UseAuthorization();

app.MapRazorPages();

app.Run();