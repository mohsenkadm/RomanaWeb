                               
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Models.EntityMap
{
    public class ImagesMap : IEntityTypeConfiguration<Images>
    {
        public void Configure(EntityTypeBuilder<Images> builder)
        {
            builder.ToTable("Images", "dbo");
            builder.HasKey(x => x.ImageId);           
            builder.Property(x => x.ImagePath).IsRequired();
            builder.Property(x => x.ProductsId).IsRequired(); 
        }
    }
}
