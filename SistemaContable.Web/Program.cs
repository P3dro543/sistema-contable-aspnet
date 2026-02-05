using SistemaContable.Repository;
using SistemaContable.Services;

var builder = WebApplication.CreateBuilder(args);

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
// NUEVO GERALD 
builder.Services.AddScoped<EstadoAsientoRepository>(sp =>
    new EstadoAsientoRepository(connectionString));
builder.Services.AddScoped<PeriodoContableRepository>(sp =>
    new PeriodoContableRepository(connectionString));
builder.Services.AddScoped<CierreContableRepository>(sp => 
    new CierreContableRepository(connectionString));

// Servicios
builder.Services.AddScoped<PantallaService>();
builder.Services.AddScoped<RolService>();
// NUEVO GERALD 
builder.Services.AddScoped<EstadoAsientoService>();
builder.Services.AddScoped<PeriodoContableService>();
builder.Services.AddScoped<CierreContableService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.Run();