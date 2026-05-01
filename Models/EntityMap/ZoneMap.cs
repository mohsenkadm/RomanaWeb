using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Models.EntityMap
{
    public class ZoneMap : IEntityTypeConfiguration<Zone>
    {
        public void Configure(EntityTypeBuilder<Zone> builder)
        {
            builder.ToTable("Zone", "dbo");
            builder.HasKey(x => x.ZoneId);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.GeoJson).IsRequired();
            builder.Property(x => x.IsActive).HasDefaultValue(true);
        }
    }

    public class ZonePriceMap : IEntityTypeConfiguration<ZonePrice>
    {
        public void Configure(EntityTypeBuilder<ZonePrice> builder)
        {
            builder.ToTable("ZonePrice", "dbo");
            builder.HasKey(x => x.ZonePriceId);
            builder.Property(x => x.FromZoneId).IsRequired();
            builder.Property(x => x.ToZoneId).IsRequired();
            builder.Property(x => x.Price).HasColumnType("decimal(18,2)").IsRequired();
            builder.HasIndex(x => new { x.FromZoneId, x.ToZoneId }).IsUnique();
        }
    }
}
