using Proyecto_Grupo_7_Progra_Avanzada.Models;
using Microsoft.EntityFrameworkCore;

namespace Proyecto_Grupo_7_Progra_Avanzada.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Comercio> Comercios { get; set; }

        public DbSet<Caja> Cajas { get; set; }

        public DbSet<Bitacora> Bitacora { get; set; }

        public DbSet<Sinpes> Sinpes { get; set; }
    }
}

