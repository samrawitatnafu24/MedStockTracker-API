using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MedStockTracker.Infrastructure.Identity;
using MedStockTracker.Domain.Entities;

namespace MedStockTracker.Infrastructure.Persistence.Data
{
    public class ApplicationDbContext : IdentityDbContext<MedStockUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Medicine> Medicines { get; set; } = null!;

        public DbSet<StockHistory> StockHistories { get; set; } = null!;

        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

        public DbSet<MedicineHold> MedicineHolds { get; set; } = null!;

        public DbSet<Category> Categories { get; set; } = null!;

        public DbSet<Supplier> Suppliers { get; set; } = null!;

        public DbSet<Purchase> Purchases { get; set; } = null!;

        public DbSet<Sale> Sales { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<MedicineHold>()
                .HasKey(x => x.Id);

            builder.Entity<MedicineHold>()
                .Property(x => x.PatientName)
                .HasMaxLength(160)
                .IsRequired();

            builder.Entity<MedicineHold>()
                .HasOne(x => x.Medicine)
                .WithMany(x => x.Holds)
                .HasForeignKey(x => x.MedicineId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Medicine>()
                .Property(x => x.HospitalBranch)
                .HasMaxLength(80)
                .IsRequired();

            builder.Entity<Medicine>()
                .Property(x => x.Price)
                .HasPrecision(18, 2);

            builder.Entity<Category>()
                .Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Entity<Supplier>()
                .Property(x => x.Name)
                .HasMaxLength(160)
                .IsRequired();

            builder.Entity<Purchase>()
                .Property(x => x.UnitCost)
                .HasPrecision(18, 2);

            builder.Entity<Purchase>()
                .Property(x => x.TotalCost)
                .HasPrecision(18, 2);

            builder.Entity<Purchase>()
                .HasOne(x => x.Medicine)
                .WithMany()
                .HasForeignKey(x => x.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Purchase>()
                .HasOne(x => x.Supplier)
                .WithMany(x => x.Purchases)
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Sale>()
                .Property(x => x.UnitPrice)
                .HasPrecision(18, 2);

            builder.Entity<Sale>()
                .Property(x => x.TotalAmount)
                .HasPrecision(18, 2);

            builder.Entity<Sale>()
                .HasOne(x => x.Medicine)
                .WithMany()
                .HasForeignKey(x => x.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Medicine>()
                .HasOne(x => x.Category)
                .WithMany(x => x.Medicines)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
