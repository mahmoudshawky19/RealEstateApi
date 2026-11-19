
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tests.Services
{
    public class CategoryServiceTests
    {
        // Create In-Memory Database
        private ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        // 1. Get All Categories
        [Fact]
        public async Task GetAllCategoriesAsync_ShouldReturnAllCategories()
        {
            // Arrange
            await using var context = CreateContext();

            context.Categories.AddRange(
                new Domain.Entities.Category
                {
                    Name = "Apartments",
                    Description = "All apartments"
                },
                new Domain.Entities.Category
                {
                    Name = "Villas",
                    Description = "All villas"
                }
            );

            await context.SaveChangesAsync();

            var service = new CategoryService(context);

            // Act
            var result = await service.GetAllCategoriesAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        // 2. Get Category By Id - Exists
        [Fact]
        public async Task GetCategoryByIdAsync_ShouldReturnCategory_WhenCategoryExists()
        {
            // Arrange
            await using var context = CreateContext();

            context.Categories.Add(new Domain.Entities.Category
            {
                Id = 5,
                Name = "Apartments",
                Description = "Apartment Category"
            });

            await context.SaveChangesAsync();

            var service = new CategoryService(context);

            // Act
            var result = await service.GetCategoryByIdAsync(5);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Id);
            Assert.Equal("Apartments", result.Name);
            Assert.Equal("Apartment Category", result.Description);
        }

        // 3. Get Category By Id - Does Not Exist
        [Fact]
        public async Task GetCategoryByIdAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
        {
            // Arrange
            await using var context = CreateContext();

            var service = new CategoryService(context);

            // Act
            var result = await service.GetCategoryByIdAsync(100);

            // Assert
            Assert.Null(result);
        }

        // 4. Create Category
        [Fact]
        public async Task CreateAsync_ShouldCreateCategorySuccessfully()
        {
            // Arrange
            await using var context = CreateContext();

            var service = new CategoryService(context);

            var request = new Application.DTOs.Category.CreateCategoryDto
            {
                Name = "Villas",
                Description = "Villa Categories"
            };

            // Act
            var result = await service.CreateAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("Villas", result.Name);
            Assert.Equal("Villa Categories", result.Description);

            var categoryInDatabase = await context.Categories.FindAsync(result.Id);

            Assert.NotNull(categoryInDatabase);
        }

        // 5. Update Category - Exists
        [Fact]
        public async Task UpdateAsync_ShouldUpdateCategory_WhenCategoryExists()
        {
            // Arrange
            await using var context = CreateContext();

            context.Categories.Add(new Domain.Entities.Category
            {
                Id = 5,
                Name = "Old Name",
                Description = "Old Description"
            });

            await context.SaveChangesAsync();

            var service = new CategoryService(context);

            var request = new Application.DTOs.Category.UpdateCategoryDto
            {
                Name = "New Name",
                Description = "New Description"
            };

            // Act
            var result = await service.UpdateAsync(5, request);

            // Assert
            Assert.True(result);

            var updatedCategory = await context.Categories.FindAsync(5);

            Assert.NotNull(updatedCategory);
            Assert.Equal("New Name", updatedCategory.Name);
            Assert.Equal("New Description", updatedCategory.Description);
        }

        // 6. Update Category - Does Not Exist
        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenCategoryDoesNotExist()
        {
            // Arrange
            await using var context = CreateContext();

            var service = new CategoryService(context);

            var request = new Application.DTOs.Category.UpdateCategoryDto
            {
                Name = "New Name",
                Description = "New Description"
            };

            // Act
            var result = await service.UpdateAsync(100, request);

            // Assert
            Assert.False(result);
        }

        // 7. Delete Category - Exists
        [Fact]
        public async Task DeleteAsync_ShouldDeleteCategory_WhenCategoryExists()
        {
            // Arrange
            await using var context = CreateContext();

            context.Categories.Add(new Domain.Entities.Category
            {
                Id = 5,
                Name = "Apartments",
                Description = "Apartment Categories"
            });

            await context.SaveChangesAsync();

            var service = new CategoryService(context);

            // Act
            var result = await service.DeleteAsync(5);

            // Assert
            Assert.True(result);

            var deletedCategory = await context.Categories.FindAsync(5);

            Assert.Null(deletedCategory);
        }

        // 8. Delete Category - Does Not Exist
        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenCategoryDoesNotExist()
        {
            // Arrange
            await using var context = CreateContext();

            var service = new CategoryService(context);

            // Act
            var result = await service.DeleteAsync(100);

            // Assert
            Assert.False(result);
        }
    }
}
