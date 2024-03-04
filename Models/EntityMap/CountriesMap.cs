using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Models.EntityMap
{
    public class CountriesMap : IEntityTypeConfiguration<Countries>
    {
        public void Configure(EntityTypeBuilder<Countries> builder)
        {
            builder.ToTable("Countries", "dbo");
            builder.HasKey(x => x.CountriesId);
            builder.Property(x => x.CountriesName).IsRequired();  
        }
    }
}
