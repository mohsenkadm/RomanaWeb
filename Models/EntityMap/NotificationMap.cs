

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Model.EntityMap
{
    public class NotificationMap : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notification", "dbo");
            builder.HasKey(x => x.NotificationId);
            builder.Property(x => x.Title).IsRequired();
            builder.Property(x => x.Details).IsRequired();
            builder.Property(x => x.DateInsert).IsRequired();     
            builder.Property(x => x.Images);     
            builder.Ignore(x => x.FileChoose);     
            builder.Property(x => x.UserId).IsRequired();     
            builder.Property(x => x.ResId).IsRequired();     
        }
    }
}
