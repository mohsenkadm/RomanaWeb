 
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;     
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Model.EntityMap
{
    public class UsersMap : IEntityTypeConfiguration<Users>
    {
        public void Configure(EntityTypeBuilder<Users> builder)
        {
            builder.ToTable("Users", "dbo");
            builder.HasKey(x => x.UserId);
            builder.Property(x => x.Name).IsRequired();       
            builder.Property(x => x.Phone).IsRequired();          
            builder.Ignore(x => x.Token);                        
            builder.Property(x => x.Address);            
            builder.Property(x => x.FunctionPoint);           
            builder.Property(x => x.Lat);           
            builder.Property(x => x.Long);           
            builder.Property(x => x.CityId);           
            builder.Property(x => x.IsActive);           
            builder.Property(x => x.IsConfirm);           
            builder.Property(x => x.IsDelete);           
            builder.Property(x => x.Code);           
            builder.Property(x => x.Password);           
        }
    }
}
