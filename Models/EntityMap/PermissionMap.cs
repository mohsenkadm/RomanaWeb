                        
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RomanaWeb.Models.Entity;     

namespace RomanaWeb.Model.EntityMap
{
    public class PermissionMap : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("Permission", "dbo");
            builder.HasKey(x => x.PermissionId);
            builder.Property(x => x.PermissionNameId).IsRequired();
            builder.Property(x => x.AdminId).IsRequired();
            builder.Ignore(x => x.PermissionName);
            builder.Ignore(x => x.ControlName);
            builder.Ignore(x => x.UserName);
            builder.Property(x => x.State).HasDefaultValue(true).IsRequired(); 
        }
    }
}
