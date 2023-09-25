using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Model.EntityMap
{
    public class StarsMap : IEntityTypeConfiguration<Stars>
    {
        public void Configure(EntityTypeBuilder<Stars> builder)
        {
            builder.ToTable("Stars", "dbo");
            builder.HasKey(x => x.StarsId);
            builder.Property(x => x.UserId);
            builder.Property(x => x.StarsCount).IsRequired();
            builder.Property(x => x.RestaurantId).IsRequired();   
            builder.Property(x => x.Comments).IsRequired();
            builder.Ignore(x => x.UserName);        
            builder.Ignore(x => x.RestaurantName);
        }
    }
}