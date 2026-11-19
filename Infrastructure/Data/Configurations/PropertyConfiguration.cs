using Domain.Entities;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Configurations
{
    public class PropertyConfiguration : IEntityTypeConfiguration<Property>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Domain.Entities.Property> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Title)
                   .IsRequired()
                   .HasMaxLength(200); 
            builder.Property(p => p.Description) 
                   .HasMaxLength(2000);
            builder.Property(p => p.Price)
                   .IsRequired();
            builder.Property(p => p.AreaSize)
                   .IsRequired();
            builder.Property(p => p.Bedrooms)
          .IsRequired(false);

            builder.Property(p => p.Bathrooms)
                   .IsRequired(false);
            builder.HasOne(p => p.Category)
                   .WithMany(c => c.Properties)
                   .HasForeignKey(p => p.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Area)
                     .WithMany(a => a.Properties)
                     .HasForeignKey(p => p.AreaId)
                     .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<ApplicationUser>()
                     .WithMany()
                     .HasForeignKey(p => p.UserId)
                     .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
