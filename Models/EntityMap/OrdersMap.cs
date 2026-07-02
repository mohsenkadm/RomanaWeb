                            
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
            builder.Property(x => x.IsDelivered) ;    
            builder.Property(x => x.IsNotDelivered) ;    
            builder.Property(x => x.IsWaiting) ;    
            builder.Property(x => x.Reason) ;    
            builder.Property(x => x.Reason2) ;
            builder.Property(x => x.IsPreparing);
            builder.Property(x => x.IsDriverEnRouteToPickup);
            builder.Property(x => x.IsPickedUpFromRestaurant);
            builder.Property(x => x.IsOutForDelivery);
            builder.Property(x => x.IsDeliveryConfirmed);
            builder.Property(x => x.Lat);
            builder.Property(x => x.Long);
            builder.Ignore(x => x.Driver);
            builder.Ignore(x => x.RestaurantLat);
            builder.Ignore(x => x.RestaurantLong);
            builder.Ignore(x => x.DropoffLat);
            builder.Ignore(x => x.DropoffLng);
            builder.Ignore(x => x.UserName);    
            builder.Ignore(x => x.RestaurantName);    
            builder.Ignore(x => x.CategoriesName);    
            builder.Ignore(x => x.SaleManName);    
            builder.Ignore(x => x.Address);    
            builder.Ignore(x => x.FunctionPoint);    
            builder.Ignore(x => x.CityName);    
            builder.Ignore(x => x.CountriesName);    
            builder.Ignore(x => x.OrderDetails);    
            builder.Ignore(x => x.Phone);    
            builder.Ignore(x => x.ResPhone);    
            builder.Ignore(x => x.Logo);    
        }
    }
}
