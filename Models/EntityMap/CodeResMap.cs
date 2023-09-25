using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Models.EntityMap
{
    public class CodeResMap : IEntityTypeConfiguration<CodeRes>
    {
        public void Configure(EntityTypeBuilder<CodeRes> builder)
        {
            builder.ToTable("CodeRes", "dbo");
            builder.HasKey(x => x.CodeResId);
            builder.Property(x => x.Code);
            builder.Property(x => x.IsFree); 
        }
    }
}

