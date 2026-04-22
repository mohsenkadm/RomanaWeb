using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Models.EntityMap
{
    public class AppSettingsMap : IEntityTypeConfiguration<AppSettings>
    {
        public void Configure(EntityTypeBuilder<AppSettings> builder)
        {
            builder.ToTable("AppSettings", "dbo");
            builder.HasKey(x => x.AppSettingsId);
            builder.Property(x => x.PricePerKm).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(x => x.DefaultOrderCost).HasColumnType("decimal(18,2)").IsRequired();
        }
    }
}
