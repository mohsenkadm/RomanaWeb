using RomanaWeb.Models.Entity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace RomanaWeb.Models.EntityMap
{
    public class RestaurantMap : IEntityTypeConfiguration<Restaurant>
    {
        public void Configure(EntityTypeBuilder<Restaurant> builder)
        {
            builder.ToTable("Restaurant", "dbo");
            builder.HasKey(x => x.RestaurantId);
            builder.Property(x => x.Name) ;
            builder.Property(x => x.Address) ;   
            builder.Property(x => x.Details) ;
            builder.Property(x => x.Logo) ;
            builder.Property(x => x.Background) ;
            builder.Property(x => x.Phone) ;
            builder.Property(x => x.Lat) ;
            builder.Property(x => x.Long) ;     
            builder.Property(x => x.Whatsapp) ;   
            builder.Property(x => x.IsClosed) ;
            builder.Property(x => x.Code) ;
            builder.Property(x => x.UserName) ;
            builder.Property(x => x.Password) ;  
            builder.Property(x => x.CategoriesId) ; 
            builder.Property(x => x.IsStars) ; 
            builder.Property(x => x.MinimumPrice) ; 
            builder.Property(x => x.Areaname) ; 
            builder.Ignore(x => x.Token);
            builder.Ignore(x => x.StarCount);  
            builder.Ignore(x => x.CategoriesName);
        }
    }
}
