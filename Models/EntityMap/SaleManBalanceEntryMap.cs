using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Models.EntityMap
{
    public class SaleManBalanceEntryMap : IEntityTypeConfiguration<SaleManBalanceEntry>
    {
        public void Configure(EntityTypeBuilder<SaleManBalanceEntry> builder)
        {
            builder.ToTable("SaleManBalanceEntry", "dbo");
            builder.HasKey(x => x.EntryId);
            builder.Property(x => x.SaleManId).IsRequired();
            builder.Property(x => x.OrderId).IsRequired();
            builder.Property(x => x.FoodNetAmount).HasColumnType("decimal(18,2)");
            builder.Property(x => x.DeliveryFeeAmount).HasColumnType("decimal(18,2)");
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.HasIndex(x => x.OrderId).IsUnique();
            builder.HasIndex(x => x.SaleManId);
            builder.HasIndex(x => x.SettlementId);
            builder.Ignore(x => x.OrderNo);
            builder.Ignore(x => x.OrderDate);
            builder.Ignore(x => x.RestaurantName);
            builder.Ignore(x => x.UserName);
            builder.Ignore(x => x.Address);
        }
    }
}