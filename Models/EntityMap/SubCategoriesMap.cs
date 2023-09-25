using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Models.EntityMap
{
    public class SubCategoriesMap : IEntityTypeConfiguration<SubCategories>
    {
        public void Configure(EntityTypeBuilder<SubCategories> builder)
        {
            builder.ToTable("SubCategories", "dbo");
            builder.HasKey(x => x.SubCategoriesId);
            builder.Property(x => x.SubCategoriesName).IsRequired();   
        }
    }
}
