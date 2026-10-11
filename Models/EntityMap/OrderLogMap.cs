using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Models.EntityMap
{
    public class OrderLogMap : IEntityTypeConfiguration<OrderLog>
    {
        public void Configure(EntityTypeBuilder<OrderLog> builder)
        {
            builder.ToTable("OrderLog", "dbo");
            builder.HasKey(x => x.OrderLogId);
            builder.Property(x => x.OrderId).IsRequired();
            builder.Property(x => x.OrderNo);
            builder.Property(x => x.Action).HasMaxLength(60).IsRequired();
            builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Details);
            builder.Property(x => x.ActorType).HasMaxLength(20).IsRequired();
            builder.Property(x => x.ActorId);
            builder.Property(x => x.ActorName).HasMaxLength(200);
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.HasIndex(x => new { x.OrderId, x.CreatedAt });
            builder.HasIndex(x => x.CreatedAt);
        }
    }
}
