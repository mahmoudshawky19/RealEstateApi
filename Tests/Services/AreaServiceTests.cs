
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tests.Services
{
    public class AreaServiceTests
    {
        // Create In-Memory Database
        private ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        // 1. Get All Areas
        [Fact]
        public async Task GetAllAreasAsync_ShouldReturnAllAreas()
        {
            await using var context = CreateContext();

            var city = new Domain.Entities.City
            {
                Name = "Cairo"
            };

            context.Cities.Add(city);
            await context.SaveChangesAsync();

            context.Areas.AddRange(
                new Domain.Entities.Area
                {
                    Name = "Nasr City",
                    CityId = city.Id
                },
                new Domain.Entities.Area
                {
                    Name = "Heliopolis",
                    CityId = city.Id
                }
            );

            await context.SaveChangesAsync();

            var service = new AreaService(context);

            // Act
            var result = await service.GetAllAreasAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, area =>
            {
                Assert.Equal(city.Id, area.CityId);
                Assert.Equal("Cairo", area.CityName);
            });
        }

        // 2. Get Area By Id - Exists
        [Fact]
        public async Task GetAreaByIdAsync_ShouldReturnArea_WhenAreaExists()
        {
            await using var context = CreateContext();

            var city = new Domain.Entities.City
            {
                Name = "Cairo"
            };

            context.Cities.Add(city);
            await context.SaveChangesAsync();

            var area = new Domain.Entities.Area
            {
                Id = 5,
                Name = "Nasr City",
                CityId = city.Id
            };

            context.Areas.Add(area);
            await context.SaveChangesAsync();

            var service = new AreaService(context);

            // Act
            var result = await service.GetAreaByIdAsync(5);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Id);
            Assert.Equal("Nasr City", result.Name);
            Assert.Equal(city.Id, result.CityId);
            Assert.Equal("Cairo", result.CityName);
        }

        // 3. Get Area By Id - Does Not Exist
        [Fact]
        public async Task GetAreaByIdAsync_ShouldReturnNull_WhenAreaDoesNotExist()
        {
            await using var context = CreateContext();

            var service = new AreaService(context);

            // Act
            var result = await service.GetAreaByIdAsync(100);

            // Assert
            Assert.Null(result);
        }

        // 4. Create Area - Valid City
        [Fact]
        public async Task CreateAsync_ShouldCreateArea_WhenCityExists()
        {
            await using var context = CreateContext();

            var city = new Domain.Entities.City
            {
                Name = "Gharbia"
            };

            context.Cities.Add(city);
            await context.SaveChangesAsync();

            var service = new AreaService(context);

            var request = new Application.DTOs.Area.CreateAreaDto
            {
                Name = "Tanta",
                CityId = city.Id
            };

            // Act
            var result = await service.CreateAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("Tanta", result.Name);
            Assert.Equal(city.Id, result.CityId);
            Assert.Equal("Gharbia", result.CityName);

            var areaInDatabase =
                await context.Areas.FindAsync(result.Id);

            Assert.NotNull(areaInDatabase);
        }

        // 5. Create Area - Invalid City
        [Fact]
        public async Task CreateAsync_ShouldThrowKeyNotFoundException_WhenCityDoesNotExist()
        {
            await using var context = CreateContext();

            var service = new AreaService(context);

            var request = new Application.DTOs.Area.CreateAreaDto
            {
                Name = "Tanta",
                CityId = 999
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => service.CreateAsync(request)
            );
        }

        // 6. Update Area - Exists
        [Fact]
        public async Task UpdateAsync_ShouldUpdateArea_WhenAreaExists()
        {
            await using var context = CreateContext();

            var oldCity = new Domain.Entities.City
            {
                Name = "Cairo"
            };

            var newCity = new Domain.Entities.City
            {
                Name = "Gharbia"
            };

            context.Cities.AddRange(oldCity, newCity);
            await context.SaveChangesAsync();

            context.Areas.Add(new Domain.Entities.Area
            {
                Id = 5,
                Name = "Old Area",
                CityId = oldCity.Id
            });

            await context.SaveChangesAsync();

            var service = new AreaService(context);

            var request = new Application.DTOs.Area.UpdateAreaDto
            {
                Name = "Tanta",
                CityId = newCity.Id
            };

            // Act
            var result = await service.UpdateAsync(5, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Id);
            Assert.Equal("Tanta", result.Name);
            Assert.Equal(newCity.Id, result.CityId);
            Assert.Equal("Gharbia", result.CityName);

            var updatedArea = await context.Areas.FindAsync(5);

            Assert.NotNull(updatedArea);
            Assert.Equal("Tanta", updatedArea.Name);
            Assert.Equal(newCity.Id, updatedArea.CityId);
        }

        // 7. Update Area - Does Not Exist
        [Fact]
        public async Task UpdateAsync_ShouldReturnNull_WhenAreaDoesNotExist()
        {
            await using var context = CreateContext();

            var service = new AreaService(context);

            var request = new Application.DTOs.Area.UpdateAreaDto
            {
                Name = "New Area",
                CityId = 1
            };

            // Act
            var result = await service.UpdateAsync(100, request);

            // Assert
            Assert.Null(result);
        }

        // 8. Update Area - Invalid City
        [Fact]
        public async Task UpdateAsync_ShouldThrowKeyNotFoundException_WhenCityDoesNotExist()
        {
            await using var context = CreateContext();

            var city = new Domain.Entities.City
            {
                Name = "Cairo"
            };

            context.Cities.Add(city);
            await context.SaveChangesAsync();

            context.Areas.Add(new Domain.Entities.Area
            {
                Id = 5,
                Name = "Old Area",
                CityId = city.Id
            });

            await context.SaveChangesAsync();

            var service = new AreaService(context);

            var request = new Application.DTOs.Area.UpdateAreaDto
            {
                Name = "New Area",
                CityId = 999
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => service.UpdateAsync(5, request)
            );
        }

        // 9. Delete Area - Exists
        [Fact]
        public async Task DeleteAsync_ShouldDeleteArea_WhenAreaExists()
        {
            await using var context = CreateContext();

            var city = new Domain.Entities.City
            {
                Name = "Cairo"
            };

            context.Cities.Add(city);
            await context.SaveChangesAsync();

            context.Areas.Add(new Domain.Entities.Area
            {
                Id = 5,
                Name = "Nasr City",
                CityId = city.Id
            });

            await context.SaveChangesAsync();

            var service = new AreaService(context);

            // Act
            var result = await service.DeleteAsync(5);

            // Assert
            Assert.True(result);

            var deletedArea = await context.Areas.FindAsync(5);

            Assert.Null(deletedArea);
        }

        // 10. Delete Area - Does Not Exist
        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenAreaDoesNotExist()
        {
            await using var context = CreateContext();

            var service = new AreaService(context);

            // Act
            var result = await service.DeleteAsync(100);

            // Assert
            Assert.False(result);
        }
    }
}
