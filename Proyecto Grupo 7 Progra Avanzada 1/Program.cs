using Microsoft.EntityFrameworkCore;
using Proyecto_Grupo_7_Progra_Avanzada.Controllers;
using Proyecto_Grupo_7_Progra_Avanzada.Data;

var builder = WebApplication.CreateBuilder(args);

// Configurar la conexión a MySQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySqlConnection"),
        new MySqlServerVersion(new Version(8, 0, 36))
    )
);



// Agregar servicios MVC
builder.Services.AddControllersWithViews();

//BitacoraController como Servicio
builder.Services.AddScoped<BitacoraController>();

var app = builder.Build();

// Configuración del pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
