using Domain.Entities;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Data.Configurations
{
    public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
    {
        public void Configure(EntityTypeBuilder<Favorite> builder)
        {
            // Composite Primary Key
            builder.HasKey(f => new { f.UserId, f.PropertyId });

            // Relationship with Property
            builder
                .HasOne(f => f.Property)
                .WithMany(p => p.Favorites)
                .HasForeignKey(f => f.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship with ApplicationUser
            builder.HasOne<ApplicationUser>()
     .WithMany()
     .HasForeignKey(f => f.UserId)
     .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
