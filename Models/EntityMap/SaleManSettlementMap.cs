using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Models.EntityMap
{
    public class SaleManSettlementMap : IEntityTypeConfiguration<SaleManSettlement>
    {
        public void Configure(EntityTypeBuilder<SaleManSettlement> builder)
        {
            builder.ToTable("SaleManSettlement", "dbo");
            builder.HasKey(x => x.SettlementId);
            builder.Property(x => x.SaleManId).IsRequired();
            builder.Property(x => x.FoodNetAmount).HasColumnType("decimal(18,2)");
            builder.Property(x => x.DeliveryFeeAmount).HasColumnType("decimal(18,2)");
            builder.Property(x => x.SettledAt).IsRequired();
            builder.Property(x => x.Note).HasMaxLength(500);
            builder.Ignore(x => x.SaleManName);
            builder.Ignore(x => x.SaleManPhone);
            builder.Ignore(x => x.AdminName);
            builder.Ignore(x => x.CurrentFoodNetBalance);
            builder.Ignore(x => x.CurrentDeliveryFeeBalance);
            builder.Ignore(x => x.OrdersCount);
            builder.HasIndex(x => x.SaleManId);
            builder.HasIndex(x => x.SettledAt);
        }
    }
}
