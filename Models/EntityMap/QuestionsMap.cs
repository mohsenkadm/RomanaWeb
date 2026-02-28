using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Model.EntityMap
{
    public class QuestionsMap : IEntityTypeConfiguration<Questions>
    {
        public void Configure(EntityTypeBuilder<Questions> builder)
        {
            builder.ToTable("Questions", "dbo");
            builder.HasKey(x => x.QuestionId);
            builder.Property(x => x.QuestionText).IsRequired().HasMaxLength(1000);
            builder.Property(x => x.AnswerText).IsRequired();
            builder.Property(x => x.IsShow).IsRequired();
            builder.Property(x => x.DisplayOrder).IsRequired();
        }
    }
}
