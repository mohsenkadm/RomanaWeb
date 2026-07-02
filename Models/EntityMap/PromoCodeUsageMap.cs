using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Models.EntityMap
{
    public class PromoCodeUsageMap : IEntityTypeConfiguration<PromoCodeUsage>
    {
        public void Configure(EntityTypeBuilder<PromoCodeUsage> builder)
        {
            builder.ToTable("PromoCodeUsage", "dbo");
            builder.HasKey(x => x.PromoCodeUsageId);
            builder.HasIndex(x => new { x.PromoCodeId, x.UserId }).IsUnique();
        }
    }
}
