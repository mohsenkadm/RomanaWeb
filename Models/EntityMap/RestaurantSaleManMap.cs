using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Models.EntityMap
{
    public class RestaurantSaleManMap : IEntityTypeConfiguration<RestaurantSaleMan>
    {
        public void Configure(EntityTypeBuilder<RestaurantSaleMan> builder)
        {
            builder.ToTable("RestaurantSaleMan", "dbo");
            builder.HasKey(x => x.RestaurantSaleManId);
            builder.Property(x => x.SaleManId).IsRequired();
            builder.Property(x => x.RestaurantId).IsRequired();
            builder.Ignore(x => x.RestaurantName);
            builder.Ignore(x => x.SaleManName);
        }
    }
}
