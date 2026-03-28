using GestionUsuariosAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionUsuariosAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Rol> Roles => Set<Rol>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Índices únicos
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email).IsUnique();

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.NombreUsuario).IsUnique();

            modelBuilder.Entity<Rol>()
                .HasIndex(r => r.Nombre).IsUnique();

            // Datos semilla
            modelBuilder.Entity<Rol>().HasData(
                new Rol { Id = 1, Nombre = "Administrador", Descripcion = "Acceso total al sistema", Activo = true },
                new Rol { Id = 2, Nombre = "Editor", Descripcion = "Puede crear y editar contenido", Activo = true },
                new Rol { Id = 3, Nombre = "Lector", Descripcion = "Solo lectura de información", Activo = true }
            );

            modelBuilder.Entity<Usuario>().HasData(
                new Usuario
                {
                    Id = 1,
                    NombreCompleto = "Admin del Sistema",
                    Email = "admin@sistema.com",
                    NombreUsuario = "admin",
                    PasswordHash = BCryptSimple.HashPassword("Admin123"),
                    Telefono = "+57 300 123 4567",
                    Activo = true,
                    RolId = 1,
                    FechaRegistro = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Usuario
                {
                    Id = 2,
                    NombreCompleto = "María García López",
                    Email = "maria.garcia@correo.com",
                    NombreUsuario = "mgarcia",
                    PasswordHash = BCryptSimple.HashPassword("Maria123"),
                    Telefono = "+57 310 987 6543",
                    Activo = true,
                    RolId = 2,
                    FechaRegistro = new DateTime(2025, 3, 15, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }

    /// <summary>
    /// Hash simple para la práctica (en producción usar BCrypt real)
    /// </summary>
    public static class BCryptSimple
    {
        public static string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password + "_salt_practica"));
            return Convert.ToBase64String(bytes);
        }

        public static bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}
