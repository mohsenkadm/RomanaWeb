                          
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Model.EntityMap
{
    public class ProductsMap : IEntityTypeConfiguration<Products>
    {
        public void Configure(EntityTypeBuilder<Products> builder)
        {
            builder.ToTable("Products", "dbo");
            builder.HasKey(x => x.ProductsId);
            builder.Property(x => x.ProductsName);
            builder.Property(x => x.ProductsDetails);
            builder.Property(x => x.ProductsPrice); 
            builder.Property(x => x.RestaurantId).IsRequired(); 
            builder.Property(x => x.ProductsImage);              
            builder.Property(x => x.SubCategoriesId).IsRequired();    
            builder.Property(x => x.IsFree);    
            builder.Ignore(x => x.SubCategoriesName);        
            builder.Ignore(x => x.RestaurantName);            
            builder.Ignore(x => x.Logo);            
            builder.Ignore(x => x.Background);            
        }
    }
}
