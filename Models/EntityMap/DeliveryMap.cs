using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Models.EntityMap
{
    public class DeliveryMap : IEntityTypeConfiguration<Delivery>
    {
        public void Configure(EntityTypeBuilder<Delivery> builder)
        {
            builder.ToTable("Delivery", "dbo");
            builder.HasKey(x => x.DeliveryId);
            builder.Property(x => x.No);
            builder.Property(x => x.Name);
            builder.Property(x => x.Phone);
            builder.Property(x => x.Address);
            builder.Property(x => x.FunctionPoint);
            builder.Ignore(x => x.RestaurantName);
            builder.Property(x => x.CostDelivery);
            builder.Property(x => x.Notes);
            builder.Property(x => x.NetAmount);
            builder.Property(x => x.RestaurantId);     
            builder.Property(x => x.CityId);
            builder.Ignore(x => x.CityName);  
            builder.Property(x => x.CountriesId).IsRequired();  
            builder.Ignore(x => x.CountriesName);  
        }
    }
}
