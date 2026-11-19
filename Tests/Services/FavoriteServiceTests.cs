
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tests.Services
{
    public class FavoriteServiceTests
    {
        // Create In-Memory Database
        private ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        // Helper: Create Property
        private Domain.Entities.Property CreateProperty(
            int id,
            string title,
            decimal price,
            string userId)
        {
            return new Domain.Entities.Property
            {
                Id = id,
                Title = title,
                Price = price,
                AreaSize = 100,
                UserId = userId,
                CategoryId = 1,
                AreaId = 1,
                Images = new List<Domain.Entities.PropertyImage>()
            };
        }

        // 1. Get All Favorites - Filter By User
        [Fact]
        public async Task GetAllFavoritesAsync_ShouldReturnOnlyUserFavorites()
        {
            await using var context = CreateContext();

            var property1 = CreateProperty(1, "Apartment", 1000000, "owner1");
            var property2 = CreateProperty(2, "Villa", 2000000, "owner2");

            context.Properties.AddRange(property1, property2);

            context.Favorites.AddRange(
                new Domain.Entities.Favorite
                {
                    UserId = "user1",
                    PropertyId = 1
                },
                new Domain.Entities.Favorite
                {
                    UserId = "user2",
                    PropertyId = 2
                }
            );

            await context.SaveChangesAsync();

            var service = new FavoriteService(context);

            // Act
            var result = await service.GetAllFavoritesAsync("user1");

            // Assert
            var favorites = result.ToList();

            Assert.Single(favorites);
            Assert.Equal(1, favorites[0].PropertyId);
            Assert.Equal("Apartment", favorites[0].PropertyTitle);
            Assert.Equal(1000000, favorites[0].Price);
        }

        // 2. Is Favorite - Exists
        [Fact]
        public async Task IsFavoriteAsync_ShouldReturnTrue_WhenFavoriteExists()
        {
            await using var context = CreateContext();

            var property = CreateProperty(1, "Apartment", 1000000, "owner1");

            context.Properties.Add(property);

            context.Favorites.Add(new Domain.Entities.Favorite
            {
                UserId = "user1",
                PropertyId = 1
            });

            await context.SaveChangesAsync();

            var service = new FavoriteService(context);

            // Act
            var result = await service.IsFavoriteAsync("user1", 1);

            // Assert
            Assert.True(result);
        }

        // 3. Is Favorite - Does Not Exist
        [Fact]
        public async Task IsFavoriteAsync_ShouldReturnFalse_WhenFavoriteDoesNotExist()
        {
            await using var context = CreateContext();

            var service = new FavoriteService(context);

            // Act
            var result = await service.IsFavoriteAsync("user1", 100);

            // Assert
            Assert.False(result);
        }

        // 4. Add To Favorites - Success
        [Fact]
        public async Task AddToFavoritesAsync_ShouldAddFavoriteSuccessfully()
        {
            await using var context = CreateContext();

            var property = CreateProperty(1, "Apartment", 1000000, "owner1");

            property.Images.Add(new Domain.Entities.PropertyImage
            {
                ImageUrl = "/images/apartment.jpg",
                IsPrimary = true,
                DisplayOrder = 1
            });

            context.Properties.Add(property);
            await context.SaveChangesAsync();

            var service = new FavoriteService(context);

            // Act
            var result = await service.AddToFavoritesAsync("user1", 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.PropertyId);
            Assert.Equal("Apartment", result.PropertyTitle);
            Assert.Equal(1000000, result.Price);
            Assert.Equal("/images/apartment.jpg", result.PrimaryImageUrl);

            var favoriteInDatabase = await context.Favorites
                .FirstOrDefaultAsync(f =>
                    f.UserId == "user1" && f.PropertyId == 1);

            Assert.NotNull(favoriteInDatabase);
        }

        // 5. Add To Favorites - Property Does Not Exist
        [Fact]
        public async Task AddToFavoritesAsync_ShouldThrowKeyNotFoundException_WhenPropertyDoesNotExist()
        {
            await using var context = CreateContext();

            var service = new FavoriteService(context);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => service.AddToFavoritesAsync("user1", 100)
            );
        }

        // 6. Add To Favorites - Already Exists
        [Fact]
        public async Task AddToFavoritesAsync_ShouldThrowInvalidOperationException_WhenFavoriteAlreadyExists()
        {
            await using var context = CreateContext();

            var property = CreateProperty(1, "Apartment", 1000000, "owner1");

            context.Properties.Add(property);

            context.Favorites.Add(new Domain.Entities.Favorite
            {
                UserId = "user1",
                PropertyId = 1
            });

            await context.SaveChangesAsync();

            var service = new FavoriteService(context);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.AddToFavoritesAsync("user1", 1)
            );
        }

        // 7. Remove From Favorites - Exists
        [Fact]
        public async Task RemoveFromFavoritesAsync_ShouldRemoveFavorite_WhenFavoriteExists()
        {
            await using var context = CreateContext();

            var property = CreateProperty(1, "Apartment", 1000000, "owner1");

            context.Properties.Add(property);

            context.Favorites.Add(new Domain.Entities.Favorite
            {
                UserId = "user1",
                PropertyId = 1
            });

            await context.SaveChangesAsync();

            var service = new FavoriteService(context);

            // Act
            var result = await service.RemoveFromFavoritesAsync("user1", 1);

            // Assert
            Assert.True(result);

            var favoriteInDatabase = await context.Favorites
                .FirstOrDefaultAsync(f =>
                    f.UserId == "user1" && f.PropertyId == 1);

            Assert.Null(favoriteInDatabase);
        }

        // 8. Remove From Favorites - Does Not Exist
        [Fact]
        public async Task RemoveFromFavoritesAsync_ShouldReturnFalse_WhenFavoriteDoesNotExist()
        {
            await using var context = CreateContext();

            var service = new FavoriteService(context);

            // Act
            var result = await service.RemoveFromFavoritesAsync("user1", 100);

            // Assert
            Assert.False(result);
        }
    }
}
