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
            builder.Property(x => x.BaseDeliveryPrice).HasColumnType("decimal(18,0)");
            builder.Property(x => x.LzaKm).HasColumnType("decimal(6,2)").HasDefaultValue(3m);
            builder.Property(x => x.EcaPricePerKm).HasColumnType("decimal(18,0)").HasDefaultValue(250m);
            builder.Property(x => x.MaxEcaFee).HasColumnType("decimal(18,0)").HasDefaultValue(2500m);
            builder.Property(x => x.MaxTotalDeliveryFee).HasColumnType("decimal(18,0)");
            builder.Property(x => x.NearRestaurantPrice).HasColumnType("decimal(18,0)");
            builder.Property(x => x.NearRestaurantKm).HasColumnType("decimal(4,2)").HasDefaultValue(1m);
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

    public class RestaurantZoneMap : IEntityTypeConfiguration<RestaurantZone>
    {
        public void Configure(EntityTypeBuilder<RestaurantZone> builder)
        {
            builder.ToTable("RestaurantZone", "dbo");
            builder.HasKey(x => x.RestaurantZoneId);
            builder.HasIndex(x => new { x.RestaurantId, x.ZoneId }).IsUnique();
        }
    }

    public class SaleManZoneMap : IEntityTypeConfiguration<SaleManZone>
    {
        public void Configure(EntityTypeBuilder<SaleManZone> builder)
        {
            builder.ToTable("SaleManZone", "dbo");
            builder.HasKey(x => x.SaleManZoneId);
            builder.HasIndex(x => new { x.SaleManId, x.ZoneId }).IsUnique();
        }
    }

    public class ServiceCoverageRequestMap : IEntityTypeConfiguration<ServiceCoverageRequest>
    {
        public void Configure(EntityTypeBuilder<ServiceCoverageRequest> builder)
        {
            builder.ToTable("ServiceCoverageRequest", "dbo");
            builder.HasKey(x => x.ServiceCoverageRequestId);
            builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Phone).HasMaxLength(20).IsRequired();
            builder.Property(x => x.Address).HasMaxLength(500);
        }
    }
}
