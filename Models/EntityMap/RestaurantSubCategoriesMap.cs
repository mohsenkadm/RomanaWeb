using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Models.EntityMap
{
    public class RestaurantSubCategoriesMap : IEntityTypeConfiguration<RestaurantSubCategories>
    {
        public void Configure(EntityTypeBuilder<RestaurantSubCategories> builder)
        {
            builder.ToTable("RestaurantSubCategories", "dbo");
            builder.HasKey(x => x.RestaurantSubCategoriesId);
            builder.Property(x => x.SubCategoriesId).IsRequired();
            builder.Property(x => x.RestaurantId).IsRequired();
            builder.Ignore(x => x.RestaurantName);
            builder.Ignore(x => x.SubCategoriesName);
        }
    }
}
