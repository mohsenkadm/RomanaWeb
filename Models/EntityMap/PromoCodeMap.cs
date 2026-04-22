using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Models.EntityMap
{
    public class PromoCodeMap : IEntityTypeConfiguration<PromoCode>
    {
        public void Configure(EntityTypeBuilder<PromoCode> builder)
        {
            builder.ToTable("PromoCode", "dbo");
            builder.HasKey(x => x.PromoCodeId);
            builder.Property(x => x.PromoName).IsRequired();  
            builder.Property(x => x.Percentage).IsRequired();  
            builder.Property(x => x.RestaurantId).IsRequired();  
            builder.Property(x => x.MaxOrders).HasDefaultValue(0);
            builder.Property(x => x.UsedOrders).HasDefaultValue(0);
            builder.Property(x => x.IsActive).HasDefaultValue(true);
            builder.Property(x => x.IsForAllStores).HasDefaultValue(false);
            builder.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            builder.Ignore(x => x.RestaurantName);  
        }
    }
}
