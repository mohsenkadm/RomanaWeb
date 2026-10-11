using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Models.EntityMap
{
    public class SaleManBalanceMap : IEntityTypeConfiguration<SaleManBalance>
    {
        public void Configure(EntityTypeBuilder<SaleManBalance> builder)
        {
            builder.ToTable("SaleManBalance", "dbo");
            builder.HasKey(x => x.SaleManId);
            builder.Property(x => x.FoodNetBalance).HasColumnType("decimal(18,2)");
            builder.Property(x => x.DeliveryFeeBalance).HasColumnType("decimal(18,2)");
            builder.Property(x => x.UpdatedAt).IsRequired();
        }
    }
}
