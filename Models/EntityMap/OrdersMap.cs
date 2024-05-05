                            
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
            builder.Property(x => x.OrderDate) ; 
            builder.Property(x => x.UserId) ; 
            builder.Property(x => x.RestaurantId) ; 
            builder.Property(x => x.Total) ; 
            builder.Property(x => x.TotalDiscount) ; 
            builder.Property(x => x.NetAmount) ;       
            builder.Property(x => x.IsDone) ;       
            builder.Property(x => x.Notes) ;       
            builder.Property(x => x.PromoCode) ;       
            builder.Property(x => x.IsDone) ;       
            builder.Property(x => x.IsApporve) ;       
            builder.Property(x => x.IsCancel) ;    
            builder.Property(x => x.IsSaleManApprove) ;    
            builder.Property(x => x.IsSaleManCancel) ;    
            builder.Property(x => x.SaleManId) ;    
            builder.Property(x => x.CostDelivery) ;    
            builder.Ignore(x => x.UserName);    
            builder.Ignore(x => x.RestaurantName);    
            builder.Ignore(x => x.CategoriesName);    
            builder.Ignore(x => x.SaleManName);    
            builder.Ignore(x => x.Address);    
            builder.Ignore(x => x.Long);    
            builder.Ignore(x => x.Lat);    
            builder.Ignore(x => x.FunctionPoint);    
            builder.Ignore(x => x.CityName);    
            builder.Ignore(x => x.CountriesName);    
            builder.Ignore(x => x.OrderDetails);    
            builder.Ignore(x => x.Phone);    
            builder.Ignore(x => x.Logo);    
        }
    }
}
