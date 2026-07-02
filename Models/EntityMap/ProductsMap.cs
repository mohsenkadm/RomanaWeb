                          
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
            builder.Ignore(x => x.ProductsImageFirst);              
            builder.Property(x => x.SubCategoriesId).IsRequired();    
            builder.Property(x => x.IsFree);
            builder.Property(x => x.PreparationTimeMinutes).HasDefaultValue(15);
            builder.Property(x => x.IsAvailable).HasDefaultValue(true);
            builder.Ignore(x => x.SubCategoriesName);        
            builder.Ignore(x => x.RestaurantName);            
            builder.Ignore(x => x.Logo);            
            builder.Ignore(x => x.Background);            
            builder.Ignore(x => x.Images);            
            builder.Ignore(x => x.Sizes);
            builder.Ignore(x => x.Ingredients);
        }
    }
}
