using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Models.EntityMap
{
    public class SupportPhoneMap : IEntityTypeConfiguration<SupportPhone>
    {
        public void Configure(EntityTypeBuilder<SupportPhone> builder)
        {
            builder.ToTable("SupportPhone", "dbo");
            builder.HasKey(x => x.SupportPhoneId);
            builder.Property(x => x.AppType).IsRequired();
            builder.Property(x => x.Phone).HasMaxLength(20).IsRequired();
            builder.Property(x => x.Label).HasMaxLength(200);
            builder.Property(x => x.IsActive).HasDefaultValue(true);
            builder.HasIndex(x => x.AppType).IsUnique();
        }
    }
}
