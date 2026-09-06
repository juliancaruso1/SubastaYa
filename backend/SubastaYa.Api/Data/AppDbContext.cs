using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SubastaYa.Api.Enums;
using SubastaYa.Api.Models;

namespace SubastaYa.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Subasta> Subastas => Set<Subasta>();
    public DbSet<Puja> Pujas => Set<Puja>();
    public DbSet<Billetera> Billeteras => Set<Billetera>();
    public DbSet<TransaccionLedger> TransaccionesLedger => Set<TransaccionLedger>();
    public DbSet<AuditoriaLog> AuditoriaLogs => Set<AuditoriaLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Billetera)
            .WithOne(b => b.Usuario)
            .HasForeignKey<Billetera>(b => b.UsuarioId);

        modelBuilder.Entity<Subasta>()
            .HasOne(s => s.Vendedor)
            .WithMany(u => u.SubastasPublicadas)
            .HasForeignKey(s => s.VendedorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Subasta>()
            .HasOne(s => s.Categoria)
            .WithMany(c => c.Subastas)
            .HasForeignKey(s => s.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Optimistic locking manual sobre Subasta y Billetera: el campo Version
        // se trata como ConcurrencyToken. En cada escritura se compara el valor
        // leído contra el de la base; si no coincide, EF lanza
        // DbUpdateConcurrencyException (ver AuctionService / WalletService).
        modelBuilder.Entity<Subasta>()
            .Property(s => s.Version)
            .IsConcurrencyToken();

        modelBuilder.Entity<Billetera>()
            .Property(b => b.Version)
            .IsConcurrencyToken();

        modelBuilder.Entity<Puja>()
            .HasOne(p => p.Subasta)
            .WithMany(s => s.Pujas)
            .HasForeignKey(p => p.SubastaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Puja>()
            .HasOne(p => p.Comprador)
            .WithMany(u => u.Pujas)
            .HasForeignKey(p => p.CompradorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TransaccionLedger>()
            .HasOne(t => t.Billetera)
            .WithMany(b => b.Movimientos)
            .HasForeignKey(t => t.BilleteraId)
            .OnDelete(DeleteBehavior.Restrict);

        // Precisión decimal explícita (evita warnings y truncamientos silenciosos)
        modelBuilder.Entity<Subasta>().Property(s => s.PrecioBase).HasPrecision(18, 2);
        modelBuilder.Entity<Subasta>().Property(s => s.IncrementoMinimo).HasPrecision(18, 2);
        modelBuilder.Entity<Puja>().Property(p => p.Monto).HasPrecision(18, 2);
        modelBuilder.Entity<Billetera>().Property(b => b.SaldoTotal).HasPrecision(18, 2);
        modelBuilder.Entity<Billetera>().Property(b => b.SaldoRetenido).HasPrecision(18, 2);
        modelBuilder.Entity<TransaccionLedger>().Property(t => t.Monto).HasPrecision(18, 2);

        SeedData(modelBuilder);

// SQLite no guarda el DateTimeKind: al leer, EF Core devuelve fechas
// con Kind=Unspecified. Si System.Text.Json las serializa así (sin "Z"),
// el navegador las interpreta como hora LOCAL en vez de UTC, corriendo
// todos los contadores por el huso horario del usuario. Este conversor
// fuerza Kind=Utc en cada lectura, para toda propiedad DateTime del modelo.
var utcConverter = new ValueConverter<DateTime, DateTime>(
    v => v,
    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

foreach (var entityType in modelBuilder.Model.GetEntityTypes())
{
    foreach (var property in entityType.GetProperties())
    {
        if (property.ClrType == typeof(DateTime))
        {
            property.SetValueConverter(utcConverter);
        }
    }
}
    }

    // 3.3 Datos Semilla Obligatorios: 4 usuarios+billeteras, 4 categorías, 5 subastas.
    // Se usan IDs fijos e HasData para que las migraciones sean reproducibles.
    private static void SeedData(ModelBuilder modelBuilder)
    {
        var ahora = DateTime.UtcNow;

        modelBuilder.Entity<Usuario>().HasData(
            new Usuario { Id = 1, Email = "vendedor@test.com", Nombre = "Vendedor Demo", PasswordHash = "demo-hash", FechaRegistro = ahora },
            new Usuario { Id = 2, Email = "comprador1@test.com", Nombre = "Comprador Uno", PasswordHash = "demo-hash", FechaRegistro = ahora },
            new Usuario { Id = 3, Email = "comprador2@test.com", Nombre = "Comprador Dos", PasswordHash = "demo-hash", FechaRegistro = ahora },
            new Usuario { Id = 4, Email = "sinfondos@test.com", Nombre = "Sin Fondos", PasswordHash = "demo-hash", FechaRegistro = ahora }
        );

        modelBuilder.Entity<Billetera>().HasData(
            new Billetera { Id = 1, UsuarioId = 1, SaldoTotal = 0m, SaldoRetenido = 0m, Version = 0 },
            new Billetera { Id = 2, UsuarioId = 2, SaldoTotal = 150000m, SaldoRetenido = 45000m, Version = 1 },
            new Billetera { Id = 3, UsuarioId = 3, SaldoTotal = 200000m, SaldoRetenido = 0m, Version = 0 },
            new Billetera { Id = 4, UsuarioId = 4, SaldoTotal = 500m, SaldoRetenido = 0m, Version = 0 }
        );

        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Nombre = "Tecnología" },
            new Categoria { Id = 2, Nombre = "Coleccionables" },
            new Categoria { Id = 3, Nombre = "Indumentaria" },
            new Categoria { Id = 4, Nombre = "Vehículos" }
        );

        modelBuilder.Entity<Subasta>().HasData(
            new Subasta
            {
                Id = 1, VendedorId = 1, CategoriaId = 1,
                Titulo = "Notebook Gamer RTX", Descripcion = "Activa estándar, con 2 pujas previas.",
                UrlImagen = "https://picsum.photos/seed/notebook/400/300",
                PrecioBase = 30000m, IncrementoMinimo = 1000m,
                FechaInicio = ahora.AddHours(-1), FechaFin = ahora.AddMinutes(25),
                Estado = EstadoSubasta.Activa, Version = 2
            },
            new Subasta
            {
                Id = 2, VendedorId = 1, CategoriaId = 2,
                Titulo = "Figura de colección edición limitada", Descripcion = "Activa crítica: cierra en menos de 2 minutos, para probar anti-sniping.",
                UrlImagen = "https://picsum.photos/seed/figura/400/300",
                PrecioBase = 5000m, IncrementoMinimo = 500m,
                FechaInicio = ahora.AddHours(-2), FechaFin = ahora.AddMinutes(5),
                Estado = EstadoSubasta.Activa, Version = 0
            },
            new Subasta
            {
                Id = 3, VendedorId = 1, CategoriaId = 4,
                Titulo = "Moto 150cc", Descripcion = "Próxima: inicio programado a +24hs, pujas bloqueadas.",
                UrlImagen = "https://picsum.photos/seed/moto/400/300",
                PrecioBase = 800000m, IncrementoMinimo = 10000m,
                FechaInicio = ahora.AddHours(24), FechaFin = ahora.AddHours(48),
                Estado = EstadoSubasta.Programada, Version = 0
            },
            new Subasta
            {
                Id = 4, VendedorId = 1, CategoriaId = 3,
                Titulo = "Campera de cuero vintage", Descripcion = "Vencida con ganador: para probar cierre y liquidación del Worker.",
                UrlImagen = "https://picsum.photos/seed/campera/400/300",
                PrecioBase = 8000m, IncrementoMinimo = 500m,
                FechaInicio = ahora.AddDays(-2), FechaFin = ahora.AddMinutes(-10),
                Estado = EstadoSubasta.Activa, Version = 1
            },
            new Subasta
            {
                Id = 5, VendedorId = 1, CategoriaId = 1,
                Titulo = "Mouse gamer inalámbrico", Descripcion = "Vencida desierta: sin pujas, para probar pase a DESIERTA.",
                UrlImagen = "https://picsum.photos/seed/mouse/400/300",
                PrecioBase = 3000m, IncrementoMinimo = 200m,
                FechaInicio = ahora.AddDays(-2), FechaFin = ahora.AddMinutes(-5),
                Estado = EstadoSubasta.Activa, Version = 0
            }
        );

        // Historial de las 2 ofertas previas en la subasta activa estándar (Id 1),
        // dejando a comprador1 (Id 2) como líder con $45.000 retenidos.
        modelBuilder.Entity<Puja>().HasData(
            new Puja { Id = 1, SubastaId = 1, CompradorId = 3, Monto = 32000m, FechaPuja = ahora.AddMinutes(-40) },
            new Puja { Id = 2, SubastaId = 1, CompradorId = 2, Monto = 45000m, FechaPuja = ahora.AddMinutes(-20) },
            // Puja ganadora ya cargada en la subasta vencida (Id 4) para simular liquidación pendiente.
            new Puja { Id = 3, SubastaId = 4, CompradorId = 3, Monto = 9500m, FechaPuja = ahora.AddDays(-1) }
        );

        // Transacciones de ledger que respaldan los depósitos y la retención de $45.000.
        modelBuilder.Entity<TransaccionLedger>().HasData(
            new TransaccionLedger { Id = 1, BilleteraId = 2, Tipo = TipoTransaccion.Deposito, Monto = 150000m, Fecha = ahora.AddDays(-3), SubastaId = null },
            new TransaccionLedger { Id = 2, BilleteraId = 3, Tipo = TipoTransaccion.Deposito, Monto = 200000m, Fecha = ahora.AddDays(-3), SubastaId = null },
            new TransaccionLedger { Id = 3, BilleteraId = 4, Tipo = TipoTransaccion.Deposito, Monto = 500m, Fecha = ahora.AddDays(-3), SubastaId = null },
            new TransaccionLedger { Id = 4, BilleteraId = 2, Tipo = TipoTransaccion.Retencion, Monto = 45000m, Fecha = ahora.AddMinutes(-20), SubastaId = 1 }
        );
    }
}
