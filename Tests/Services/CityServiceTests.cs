
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tests.Services
{
    public class CityServiceTests
    {
        // Create In-Memory Database
        private ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        // 1. Get All Cities
        [Fact]
        public async Task GetAllCitiesAsync_ShouldReturnAllCities()
        {
            await using var context = CreateContext();

            context.Cities.AddRange(
                new Domain.Entities.City
                {
                    Name = "Cairo"
                },
                new Domain.Entities.City
                {
                    Name = "Alexandria"
                }
            );

            await context.SaveChangesAsync();

            var service = new CityService(context);

            // Act
            var result = await service.GetAllCitiesAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        // 2. Get City By Id - Exists
        [Fact]
        public async Task GetCityByIdAsync_ShouldReturnCity_WhenCityExists()
        {
            await using var context = CreateContext();

            context.Cities.Add(new Domain.Entities.City
            {
                Id = 5,
                Name = "Cairo"
            });

            await context.SaveChangesAsync();

            var service = new CityService(context);

            // Act
            var result = await service.GetCityByIdAsync(5);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Id);
            Assert.Equal("Cairo", result.Name);
        }

        // 3. Get City By Id - Does Not Exist
        [Fact]
        public async Task GetCityByIdAsync_ShouldReturnNull_WhenCityDoesNotExist()
        {
            await using var context = CreateContext();

            var service = new CityService(context);

            // Act
            var result = await service.GetCityByIdAsync(100);

            // Assert
            Assert.Null(result);
        }

        // 4. Create City
        [Fact]
        public async Task CreateAsync_ShouldCreateCitySuccessfully()
        {
            await using var context = CreateContext();

            var service = new CityService(context);

            var request = new Application.DTOs.City.CreateCityDto
            {
                Name = "Tanta"
            };

            // Act
            var result = await service.CreateAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("Tanta", result.Name);

            var cityInDatabase =
                await context.Cities.FindAsync(result.Id);

            Assert.NotNull(cityInDatabase);
            Assert.Equal("Tanta", cityInDatabase.Name);
        }

        // 5. Update City - Exists
        [Fact]
        public async Task UpdateAsync_ShouldUpdateCity_WhenCityExists()
        {
            await using var context = CreateContext();

            context.Cities.Add(new Domain.Entities.City
            {
                Id = 5,
                Name = "Old City"
            });

            await context.SaveChangesAsync();

            var service = new CityService(context);

            var request = new Application.DTOs.City.UpdateCityDto
            {
                Name = "New City"
            };

            // Act
            var result = await service.UpdateAsync(5, request);

            // Assert
            Assert.True(result);

            var updatedCity = await context.Cities.FindAsync(5);

            Assert.NotNull(updatedCity);
            Assert.Equal("New City", updatedCity.Name);
        }

        // 6. Update City - Does Not Exist
        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenCityDoesNotExist()
        {
            await using var context = CreateContext();

            var service = new CityService(context);

            var request = new Application.DTOs.City.UpdateCityDto
            {
                Name = "New City"
            };

            // Act
            var result = await service.UpdateAsync(100, request);

            // Assert
            Assert.False(result);
        }

        // 7. Delete City - Exists
        [Fact]
        public async Task DeleteAsync_ShouldDeleteCity_WhenCityExists()
        {
            await using var context = CreateContext();

            context.Cities.Add(new Domain.Entities.City
            {
                Id = 5,
                Name = "Cairo"
            });

            await context.SaveChangesAsync();

            var service = new CityService(context);

            // Act
            var result = await service.DeleteAsync(5);

            // Assert
            Assert.True(result);

            var deletedCity = await context.Cities.FindAsync(5);

            Assert.Null(deletedCity);
        }

        // 8. Delete City - Does Not Exist
        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenCityDoesNotExist()
        {
            await using var context = CreateContext();

            var service = new CityService(context);

            // Act
            var result = await service.DeleteAsync(100);

            // Assert
            Assert.False(result);
        }
    }
}
