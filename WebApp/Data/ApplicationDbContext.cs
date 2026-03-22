using Microsoft.EntityFrameworkCore;
using WebApp.Models;

namespace WebApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<ContactoUsuario> ContactosUsuario { get; set; }
        public DbSet<Responsabilidad> Responsabilidades { get; set; }
        public DbSet<HistorialResponsabilidad> HistorialResponsabilidades { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relación 1 a 1 obligatoria entre Usuario y ContactoUsuario
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Contacto)
                .WithOne(c => c.Usuario)
                .HasForeignKey<ContactoUsuario>(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
