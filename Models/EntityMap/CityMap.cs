using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Models.EntityMap
{
    public class CityMap : IEntityTypeConfiguration<City>
    {
        public void Configure(EntityTypeBuilder<City> builder)
        {
            builder.ToTable("City", "dbo");
            builder.HasKey(x => x.CityId);
            builder.Property(x => x.CityName).IsRequired();  
            builder.Property(x => x.CountriesId).IsRequired();  
            builder.Ignore(x => x.CountriesName);  
        }
    }
}
