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
            builder.Property(x => x.MinChargeKmThreshold).HasColumnType("decimal(18,2)").HasDefaultValue(1.5m);
            builder.Property(x => x.MinChargeAmount).HasColumnType("decimal(18,2)").HasDefaultValue(500m);
            builder.Property(x => x.RoundingMode).HasMaxLength(10).HasDefaultValue("Ceil");
            builder.Property(x => x.ZoneMaxKm).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
            builder.Property(x => x.ZoneMinKm).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
        }
    }
}
