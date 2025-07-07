using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PersonelTakip.Models;


namespace PersonelTakip.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>  // IdentityDbContext'ten türetildi
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Diğer DbSet'ler
        
        public DbSet<Gorev> Gorevs { get; set; }

        public DbSet<GorevTalebi> GorevTalepleri { get; set; }

        public DbSet<Kurum> Kurumlar { get; set; }
        public DbSet<Birim> Birimler { get; set; }

        public DbSet<Unvan> Unvanlar { get; set; }
        public DbSet<CalismaSekli> CalismaSekli { get; set; }

        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<Duyuru> Duyurular { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.KurumNavigation)
                .WithMany(k => k.Kullanicilar)
                .HasForeignKey(u => u.KurumId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.BirimNavigation)
                .WithMany(b => b.Kullanicilar)
                .HasForeignKey(u => u.BirimId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.UnvanNavigation)
                .WithMany(u => u.Kullanicilar)
                .HasForeignKey(u => u.UnvanId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany()
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Gorev>()
                .HasOne(g => g.Kullanici)
                .WithMany()
                .HasForeignKey(g => g.KullaniciId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Gorev>()
                .HasOne(g => g.AtayanKullanici)
                .WithMany()
                .HasForeignKey(g => g.AtayanKullaniciId)
                .OnDelete(DeleteBehavior.Restrict);


        }




    }
}
