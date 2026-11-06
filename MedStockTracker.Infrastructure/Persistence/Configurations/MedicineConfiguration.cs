using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MedStockTracker.Domain.Entities;

namespace MedStockTracker.Infrastructure.Persistence.Data.Configurations
{
    public class MedicineConfiguration : IEntityTypeConfiguration<Medicine>
    {
        public void Configure(EntityTypeBuilder<Medicine> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(m => m.BatchNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(m => m.Price)
                .HasPrecision(18, 2);

            builder.Property(m => m.Quantity)
                .IsRequired();
        }
    }
}
