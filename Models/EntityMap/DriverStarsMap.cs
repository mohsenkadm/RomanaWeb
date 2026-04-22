using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Model.EntityMap
{
    public class DriverStarsMap : IEntityTypeConfiguration<DriverStars>
    {
        public void Configure(EntityTypeBuilder<DriverStars> builder)
        {
            builder.ToTable("DriverStars", "dbo");
            builder.HasKey(x => x.DriverStarsId);
            builder.Property(x => x.StarsCount).IsRequired();
            builder.Property(x => x.SaleManId).IsRequired();
            builder.Property(x => x.Comments).IsRequired();
            builder.Property(x => x.OrderId);
            builder.Property(x => x.UserName);
            builder.Ignore(x => x.SaleManName);
        }
    }
}
