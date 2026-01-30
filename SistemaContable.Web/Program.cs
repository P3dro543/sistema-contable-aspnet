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

// Servicios
builder.Services.AddScoped<PantallaService>();
builder.Services.AddScoped<RolService>();

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