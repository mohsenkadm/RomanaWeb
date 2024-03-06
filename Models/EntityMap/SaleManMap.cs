 
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;     
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Model.EntityMap
{
    public class SaleManMap : IEntityTypeConfiguration<SaleMan>
    {
        public void Configure(EntityTypeBuilder<SaleMan> builder)
        {
            builder.ToTable("SaleMan", "dbo");
            builder.HasKey(x => x.SaleManId);
            builder.Property(x => x.Name).IsRequired();       
            builder.Property(x => x.Phone).IsRequired();          
            builder.Ignore(x => x.Token);                        
            builder.Property(x => x.Address);           
            builder.Property(x => x.IsActive);             
            builder.Property(x => x.IsDelete);        
            builder.Property(x => x.RestaurantId);        
            builder.Property(x => x.Password);               
            builder.Ignore(x => x.RestaurantName);           
        }
    }
}
