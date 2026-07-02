using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Model.EntityMap
{
    public class DriverLocationMap : IEntityTypeConfiguration<DriverLocation>
    {
        public void Configure(EntityTypeBuilder<DriverLocation> builder)
        {
            builder.ToTable("DriverLocations", "dbo");
            builder.HasKey(x => x.SaleManId);
            builder.Property(x => x.Lat).IsRequired();
            builder.Property(x => x.Lng).IsRequired();
            builder.Property(x => x.UpdatedAt).IsRequired();
            builder.Property(x => x.ActiveOrderId);
        }
    }
}
