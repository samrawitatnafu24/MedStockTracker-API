using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MedStockTracker.Domain.Entities;

namespace MedStockTracker.Infrastructure.Persistence.Data.Configurations
{
    public class StockHistoryConfiguration : IEntityTypeConfiguration<StockHistory>
    {
        public void Configure(EntityTypeBuilder<StockHistory> builder)
        {
            builder.HasKey(sh => sh.Id);

            builder.Property(sh => sh.ActionType)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(sh => sh.ChangedBy)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasOne(sh => sh.Medicine)
                .WithMany()
                .HasForeignKey(sh => sh.MedicineId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
