using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Models.EntityMap
{
    public class AppSplashMap : IEntityTypeConfiguration<AppSplash>
    {
        public void Configure(EntityTypeBuilder<AppSplash> builder)
        {
            builder.ToTable("AppSplash", "dbo");
            builder.HasKey(x => x.AppSplashId);
            builder.Property(x => x.ImageUrl).HasMaxLength(500);
            builder.Property(x => x.Details).HasMaxLength(1000);
        }
    }
}
