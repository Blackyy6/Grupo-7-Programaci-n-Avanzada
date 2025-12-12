using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Proyecto_Grupo_7_Progra_Avanzada.Models;

namespace Proyecto_Grupo_7_Progra_Avanzada.Data
{
    // CAMBIO IMPORTANTE: Heredar de IdentityDbContext<ApplicationUser> para la autenticación
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Tus tablas existentes (Tal cual me las pasaste)
        public DbSet<Comercio> Comercios { get; set; }
        public DbSet<Caja> Cajas { get; set; }
        public DbSet<Bitacora> Bitacora { get; set; }

        // Nota: Asegúrate de que el nombre de la clase Modelo coincida (Sinpes vs SinpePago)
        public DbSet<Sinpes> Sinpes { get; set; }

        public DbSet<Configuracion> Configuraciones { get; set; }
        public DbSet<Reporte> Reportes { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // OBLIGATORIO: Esta línea permite que Identity configure sus tablas (AspNetUsers, etc.)
            // Si la borras, tendrás errores al crear la base de datos.
            base.OnModelCreating(builder);

            // Aquí puedes agregar configuraciones adicionales de tus modelos si es necesario


            builder.Entity<ApplicationUser>(entity => entity.ToTable(name: "AspNetUsers"));
            builder.Entity<IdentityRole>(entity => entity.ToTable(name: "AspNetRoles"));
            builder.Entity<IdentityUserRole<string>>(entity => entity.ToTable("AspNetUserRoles"));
            builder.Entity<IdentityUserClaim<string>>(entity => entity.ToTable("AspNetUserClaims"));
            builder.Entity<IdentityUserLogin<string>>(entity => entity.ToTable("AspNetUserLogins"));
            builder.Entity<IdentityRoleClaim<string>>(entity => entity.ToTable("AspNetRoleClaims"));
            builder.Entity<IdentityUserToken<string>>(entity => entity.ToTable("AspNetUserTokens"));
        }
    }
}