
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Model.EntityMap
{
    public class CarouselMap : IEntityTypeConfiguration<Carousel>
    {
        public void Configure(EntityTypeBuilder<Carousel> builder)
        {
            builder.ToTable("Carousel", "dbo");
            builder.HasKey(x => x.CarouseId);
            builder.Property(x => x.Image).IsRequired();
            builder.Property(x => x.Url);
            builder.Property(x => x.IsShow); 
            builder.Property(x => x.CountryId);
            builder.Property(x => x.AppType).HasDefaultValue(1);
            builder.Ignore(x => x.CountryName); 
        }
    }
}
