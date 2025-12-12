using Microsoft.EntityFrameworkCore;
using Proyecto_Grupo_7_Progra_Avanzada.Controllers;
using Proyecto_Grupo_7_Progra_Avanzada.Data;
// =================================================================
// USINGS REQUERIDOS
// =================================================================
using Microsoft.AspNetCore.Identity;
using Proyecto_Grupo_7_Progra_Avanzada.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
// =================================================================

var builder = WebApplication.CreateBuilder(args);

// Configurar la conexión a MySQL
var connectionString = builder.Configuration.GetConnectionString("MySqlConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 36))
    )
);

// =================================================================
// 1. CONFIGURACIÓN DE IDENTITY
// =================================================================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Configuración de la cookie de aplicación
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// =================================================================
// 2. CONFIGURACIÓN DE JWT para Autenticación de API
// =================================================================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? "LlaveSecretaJWT_Avanzada_2025_#P7zH");

builder.Services.AddAuthentication()
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});


// Agregar servicios MVC
builder.Services.AddControllersWithViews();

// >>>>>>>>>> CORRECCIÓN AQUÍ: AGREGAR SERVICIOS RAZOR PAGES <<<<<<<<<<
builder.Services.AddRazorPages();

// CLAVE: CORRECCIÓN DEL NOMBRE DEL SERVICIO DE BITÁCORA
// Debe ser BitacoraController (la clase que inyecta ReportesController), no AuthController.
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

// =================================================================
// 3. HABILITAR AUTENTICACIÓN Y AUTORIZACIÓN
// =================================================================
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();