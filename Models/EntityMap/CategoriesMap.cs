 
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Models.EntityMap
{
    public class CategoriesMap : IEntityTypeConfiguration<Categories>
    {
        public void Configure(EntityTypeBuilder<Categories> builder)
        {
            builder.ToTable("Categories", "dbo");
            builder.HasKey(x => x.CategoriesId);           
            builder.Property(x => x.CategoriesName).IsRequired();   
            builder.Property(x => x.CategoriesImages);   
        }
    }
}
