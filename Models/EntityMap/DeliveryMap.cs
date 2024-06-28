using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Models.EntityMap
{
    public class DeliveryMap : IEntityTypeConfiguration<Delivery>
    {
        public void Configure(EntityTypeBuilder<Delivery> builder)
        {
            builder.ToTable("Delivery", "dbo");
            builder.HasKey(x => x.DeliveryId);
            builder.Property(x => x.No);
            builder.Property(x => x.Name);
            builder.Property(x => x.Phone);
            builder.Property(x => x.Address);
            builder.Property(x => x.FunctionPoint);
            builder.Ignore(x => x.RestaurantName);
            builder.Property(x => x.CostDelivery);
            builder.Property(x => x.Notes);
            builder.Property(x => x.NetAmount);
            builder.Property(x => x.RestaurantId);     
            builder.Property(x => x.DateInsert);
            builder.Property(x => x.CityId);
            builder.Property(x => x.IsDelivered);
            builder.Property(x => x.IsNotDelivered);
            builder.Property(x => x.IsWaiting);
            builder.Property(x => x.Reason);
            builder.Property(x => x.Reason2);
            builder.Ignore(x => x.CityName);  
            builder.Ignore(x => x.CountriesId);  
            builder.Ignore(x => x.CountriesName);  
            builder.Ignore(x => x.ResPhone);  
        }
    }
}
