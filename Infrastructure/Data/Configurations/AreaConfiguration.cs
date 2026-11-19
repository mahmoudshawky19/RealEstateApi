using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Configurations
{
    public class AreaConfiguration : IEntityTypeConfiguration<Area>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Domain.Entities.Area> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Name)
                   .IsRequired()
                   .HasMaxLength(100);
            builder.Property(a => a.CityId)
                   .IsRequired();
            builder.HasOne(a => a.City)
                   .WithMany(c => c.Areas)
                   .HasForeignKey(a => a.CityId)
.OnDelete(DeleteBehavior.Restrict);
        }
    }
}
