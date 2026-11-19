using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Data.Configurations
{
    public class PropertyImageConfiguration : IEntityTypeConfiguration<PropertyImage>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<PropertyImage> builder)
        {
            builder.HasKey(pi => pi.Id);
            builder.HasOne(pi => pi.Property)
                   .WithMany(p => p.Images)
                   .HasForeignKey(pi => pi.PropertyId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.Property(pi => pi.ImageUrl)
                   .IsRequired()
                   .HasMaxLength(500);
        }
    }
}
