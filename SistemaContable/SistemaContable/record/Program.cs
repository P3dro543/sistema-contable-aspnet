// Program.cs (ASP.NET Core 6+)
using MySql.Data.MySqlClient;
using SistemaContable.Data.Repositories;
using SistemaContable.Services;
using System.Data;

namespace SistemaContable.Record
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services
            builder.Services.AddScoped<IDbConnection>(sp =>
                new MySqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<ICuentaContableRepository, CuentaContableRepository>();
            builder.Services.AddScoped<IBitacoraService, BitacoraService>();

            // Add MVC
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}