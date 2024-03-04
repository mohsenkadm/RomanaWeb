using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Models.EntityMap
{
    public class RestaurantCityMap : IEntityTypeConfiguration<RestaurantCity>
    {
        public void Configure(EntityTypeBuilder<RestaurantCity> builder)
        {
            builder.ToTable("RestaurantCity", "dbo");
            builder.HasKey(x => x.RestaurantCityId);
            builder.Property(x => x.CityId).IsRequired();
            builder.Property(x => x.RestaurantId).IsRequired();
            builder.Property(x => x.CostDelivery);
            builder.Ignore(x => x.RestaurantName);
            builder.Ignore(x => x.CityName);
        }
    }
}
