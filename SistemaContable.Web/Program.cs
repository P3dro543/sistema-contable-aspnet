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
builder.Services.AddScoped<PantallaRepository>(sp =>
    new PantallaRepository(connectionString));
builder.Services.AddScoped<RolRepository>(sp =>
    new RolRepository(connectionString));
builder.Services.AddScoped<BitacoraRepository>(sp =>
    new BitacoraRepository(connectionString));

builder.Services.AddScoped<SistemaContable.Repository.Auth>(sp =>
    new SistemaContable.Repository.Auth (connectionString));
builder.Services.AddScoped<UsuarioRepository>(sp =>
    new UsuarioRepository(connectionString));
// Servicios
builder.Services.AddScoped<PantallaService>();
builder.Services.AddScoped<RolService>();

builder.Services.AddScoped<SistemaContable.Services.Auth>();
builder.Services.AddScoped<UsuarioService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseSession();
app.UseMiddleware<ValidarSesionMiddleware>();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.Run();  