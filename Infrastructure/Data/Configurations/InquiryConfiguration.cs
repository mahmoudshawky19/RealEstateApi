using Domain.Entities;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Configurations
{
    public class InquiryConfiguration : IEntityTypeConfiguration< Inquiry>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder< Inquiry> builder)
        {
           builder.HasKey(i => i.Id);
           builder.Property(i => i.Message)
                  .IsRequired()
                  .HasMaxLength(2000);
            builder.HasOne<ApplicationUser>()
        .WithMany()
        .HasForeignKey(i => i.UserId)
        .OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(i => i.Property)
                     .WithMany(p => p.Inquiries)
                     .HasForeignKey(i => i.PropertyId);
         }
    }
}
