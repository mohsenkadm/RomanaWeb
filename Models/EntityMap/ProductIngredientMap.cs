using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Models.EntityMap
{
    public class ProductIngredientMap : IEntityTypeConfiguration<ProductIngredient>
    {
        public void Configure(EntityTypeBuilder<ProductIngredient> builder)
        {
            builder.ToTable("ProductIngredient", "dbo");
            builder.HasKey(x => x.ProductIngredientId);
            builder.Property(x => x.ProductsId).IsRequired();
            builder.Property(x => x.IngredientName).IsRequired().HasMaxLength(300);
        }
    }
}
