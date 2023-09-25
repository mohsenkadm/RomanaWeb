using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Models.EntityMap
{
    public class PromoCodeMap : IEntityTypeConfiguration<PromoCode>
    {
        public void Configure(EntityTypeBuilder<PromoCode> builder)
        {
            builder.ToTable("PromoCode", "dbo");
            builder.HasKey(x => x.PromoCodeId);
            builder.Property(x => x.PromoName).IsRequired();  
            builder.Property(x => x.Percentage).IsRequired();  
        }
    }
}
