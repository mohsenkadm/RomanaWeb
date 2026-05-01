using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Models.EntityMap
{
    public class ProductSizeMap : IEntityTypeConfiguration<ProductSize>
    {
        public void Configure(EntityTypeBuilder<ProductSize> builder)
        {
            builder.ToTable("ProductSize", "dbo");
            builder.HasKey(x => x.ProductSizeId);
            builder.Property(x => x.ProductsId).IsRequired();
            builder.Property(x => x.SizeName).IsRequired().HasMaxLength(200);
            builder.Property(x => x.SizePrice);
        }
    }
}
