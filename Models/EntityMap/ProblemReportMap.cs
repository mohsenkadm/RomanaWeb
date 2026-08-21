using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Models.EntityMap
{
    public class ProblemReportMap : IEntityTypeConfiguration<ProblemReport>
    {
        public void Configure(EntityTypeBuilder<ProblemReport> builder)
        {
            builder.ToTable("ProblemReport", "dbo");
            builder.HasKey(x => x.ProblemReportId);
            builder.Property(x => x.OrderId).IsRequired();
            builder.Property(x => x.SaleManId).IsRequired();
            builder.Property(x => x.Message).HasMaxLength(2000).IsRequired();
            builder.Property(x => x.Status).HasDefaultValue(0);
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.UpdatedAt);
            builder.Property(x => x.AdminNote).HasMaxLength(1000);
            builder.Ignore(x => x.OrderNo);
            builder.Ignore(x => x.SaleManName);
            builder.Ignore(x => x.SaleManPhone);
            builder.Ignore(x => x.RestaurantName);
            builder.Ignore(x => x.OrderDate);
            builder.Ignore(x => x.StatusLabel);
            builder.HasIndex(x => x.CreatedAt);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.OrderId);
            builder.HasIndex(x => x.SaleManId);
        }
    }
}
