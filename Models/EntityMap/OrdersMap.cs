                            
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Model.EntityMap
{
    public class OrdersMap : IEntityTypeConfiguration<Orders>
    {
        public void Configure(EntityTypeBuilder<Orders> builder)
        {
            builder.ToTable("Orders", "dbo");
            builder.HasKey(x => x.OrderId);
            builder.Property(x => x.OrderNo);
            builder.Property(x => x.OrderDate).IsRequired(); 
            builder.Property(x => x.UserId).IsRequired(); 
            builder.Property(x => x.RestaurantId).IsRequired(); 
            builder.Property(x => x.Total).IsRequired(); 
            builder.Property(x => x.TotalDiscount).IsRequired(); 
            builder.Property(x => x.NetAmount).IsRequired();       
            builder.Property(x => x.IsDone).IsRequired();       
            builder.Property(x => x.IsApporve).IsRequired();       
            builder.Property(x => x.IsCancel).IsRequired();    
            builder.Ignore(x => x.UserName);    
            builder.Ignore(x => x.RestaurantName);    
            builder.Ignore(x => x.CategoriesName);    
            builder.Ignore(x => x.Address);    
            builder.Ignore(x => x.FunctionPoint);    
            builder.Ignore(x => x.CategoriesName);    
            builder.Ignore(x => x.OrderDetails);    
            builder.Ignore(x => x.Phone);    
            builder.Ignore(x => x.Logo);    
        }
    }
}
