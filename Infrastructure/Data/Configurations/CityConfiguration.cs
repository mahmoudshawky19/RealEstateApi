using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Data.Configurations
{
    public class CityConfiguration : IEntityTypeConfiguration<City>
    {
        public void Configure(EntityTypeBuilder<City> builder)
        {
           builder.HasKey(c => c.Id);
            builder.Property(c => c.Name)
                   .IsRequired()
                   .HasMaxLength(100);
            builder.Property(c => c.CreatedAt)
                   .IsRequired();
         builder.HasMany(c => c.Areas)
                   .WithOne(a => a.City)
                   .HasForeignKey(a => a.CityId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
