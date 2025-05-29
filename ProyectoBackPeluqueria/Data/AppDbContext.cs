using Microsoft.EntityFrameworkCore;
using NugetProyectoBackPeluqueria.Models;
namespace ProyectoBackPeluqueria.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSets para tablas
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<RolUsuario> RolesUsuario { get; set; }
        public DbSet<Servicio> Servicios { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Compra> Compras { get; set; }
        public DbSet<CompraDetalle> CompraDetalles { get; set; }
        public DbSet<HorarioDisponible> HorariosDisponibles { get; set; }
        public DbSet<ReservaView> VistaReservas { get; set; }
        public DbSet<UsuarioView> VistaUsuarios { get; set; }


        // DbSets para vistas
        //public DbSet<VistaUsuario> VistaUsuarios { get; set; }
        //public DbSet<VistaReserva> VistaReservas { get; set; }
        //public DbSet<VistaCompra> VistaCompras { get; set; }
        //public DbSet<VistaCompraDetalle> VistaComprasDetalle { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);

        //    // Configurar vistas
        //    modelBuilder.Entity<VistaUsuario>().HasNoKey().ToView("Vista_Usuarios");
        //    modelBuilder.Entity<VistaReserva>().HasNoKey().ToView("Vista_Reservas");
        //    modelBuilder.Entity<VistaCompra>().HasNoKey().ToView("Vista_Compras");
        //    modelBuilder.Entity<VistaCompraDetalle>().HasNoKey().ToView("Vista_Compras_Detalle");
        //}
    }
}
