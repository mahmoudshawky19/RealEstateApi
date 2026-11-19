using Application.Interfaces;
using Application.DTOs.Property;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Tests.Services
{
    public class PropertyServiceTests
    {
        private ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private async Task SeedBaseDataAsync(ApplicationDbContext context)
        {
            context.Cities.Add(new City
            {
                Id = 1,
                Name = "Tanta"
            });

            context.Categories.Add(new Category
            {
                Id = 1,
                Name = "Apartment",
                Description = "Apartments"
            });

            context.Areas.Add(new Area
            {
                Id = 1,
                Name = "Saeed Street",
                CityId = 1
            });

            await context.SaveChangesAsync();
        }

        // Property Factory
        private Property CreateProperty(
            int id,
            string title,
            string userId = "user1",
            PropertyStatus status = PropertyStatus.Approved,
            decimal price = 1000000,
            int categoryId = 1,
            int areaId = 1,
            ListingType listingType = ListingType.Sale,
            bool isFeatured = false,
            DateTime? createdAt = null,
            int? bedrooms = 3,
            int? bathrooms = 2)
        {
            return new Property
            {
                Id = id,
                Title = title,
                Description = $"Description for {title}",
                Price = price,
                AreaSize = 150,
                Bedrooms = bedrooms,
                Bathrooms = bathrooms,
                ListingType = listingType,
                Status = status,
                IsFeatured = isFeatured,
                UserId = userId,
                CategoryId = categoryId,
                AreaId = areaId,
                CreatedAt = createdAt ?? DateTime.UtcNow
            };
        }

        private Mock<INotificationService> CreateNotificationMock()
        {
            var notificationMock = new Mock<INotificationService>();

            notificationMock
                .Setup(n => n.CreateNotificationAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            return notificationMock;
        }

        // 1. Get All Properties - Approved Only
        [Fact]
        public async Task GetAllPropertiesAsync_ShouldReturnOnlyApprovedProperties()
        {
            await using var context = CreateContext();
            await SeedBaseDataAsync(context);

            context.Properties.AddRange(
                CreateProperty(1, "Approved Apartment"),
                CreateProperty(2, "Pending Apartment",
                    status: PropertyStatus.Pending),
                CreateProperty(3, "Rejected Apartment",
                    status: PropertyStatus.Rejected));

            await context.SaveChangesAsync();

            var service = new PropertyService(
                context, CreateNotificationMock().Object);

            var result = await service.GetAllPropertiesAsync();

            var properties = result.ToList();

            Assert.Single(properties);
            Assert.Equal("Approved Apartment", properties[0].Title);
            Assert.Equal(PropertyStatus.Approved, properties[0].Status);
        }

        // 2. Sort Properties - Price Ascending
        [Fact]
        public async Task SortPropertiesAsync_ShouldSortByPriceAscending()
        {
            await using var context = CreateContext();
            await SeedBaseDataAsync(context);

            context.Properties.AddRange(
                CreateProperty(1, "Expensive", price: 3000000),
                CreateProperty(2, "Cheap", price: 500000),
                CreateProperty(3, "Medium", price: 1500000),
                CreateProperty(4, "Pending", price: 100000,
                    status: PropertyStatus.Pending));

            await context.SaveChangesAsync();

            var service = new PropertyService(
                context, CreateNotificationMock().Object);

            var result = await service.SortPropertiesAsync(
                new PropertySortDto { SortBy = "priceAsc" });

            var properties = result.ToList();

            Assert.Equal(3, properties.Count);
            Assert.Equal("Cheap", properties[0].Title);
            Assert.Equal("Medium", properties[1].Title);
            Assert.Equal("Expensive", properties[2].Title);
        }

        // 3. Get Featured Properties
        [Fact]
        public async Task GetAllPropertiesFeaturedAsync_ShouldReturnOnlyApprovedFeaturedProperties()
        {
            await using var context = CreateContext();
            await SeedBaseDataAsync(context);

            context.Properties.AddRange(
                CreateProperty(1, "Featured", isFeatured: true),
                CreateProperty(2, "Not Featured", isFeatured: false),
                CreateProperty(3, "Pending Featured",
                    status: PropertyStatus.Pending,
                    isFeatured: true));

            await context.SaveChangesAsync();

            var service = new PropertyService(
                context, CreateNotificationMock().Object);

            var result = await service.GetAllPropertiesFeaturedAsync();

            var properties = result.ToList();

            Assert.Single(properties);
            Assert.Equal("Featured", properties[0].Title);
        }

        // 4. Get Related Properties - Success
        [Fact]
        public async Task GetRelatedPropertiesAsync_ShouldReturnRelatedProperties()
        {
            await using var context = CreateContext();
            await SeedBaseDataAsync(context);

            context.Properties.AddRange(
                CreateProperty(1, "Main Property"),
                CreateProperty(2, "Related Property"),
                CreateProperty(3, "Different Listing Type",
                    listingType: ListingType.Rent),
                CreateProperty(4, "Pending Related",
                    status: PropertyStatus.Pending));

            await context.SaveChangesAsync();

            var service = new PropertyService(
                context, CreateNotificationMock().Object);

            var result = await service.GetRelatedPropertiesAsync(1);

            var properties = result.ToList();

            Assert.Single(properties);
            Assert.Equal("Related Property", properties[0].Title);
        }

        // 5. Get Related Properties - Not Found
        [Fact]
        public async Task GetRelatedPropertiesAsync_ShouldThrowArgumentException_WhenPropertyDoesNotExist()
        {
            await using var context = CreateContext();
            await SeedBaseDataAsync(context);

            var service = new PropertyService(
                context, CreateNotificationMock().Object);

            await Assert.ThrowsAsync<ArgumentException>(
                () => service.GetRelatedPropertiesAsync(999));
        }

        // 6. Add Property To Featured - Success
        [Fact]
        public async Task AddPropertyToFeaturedAsync_ShouldSetIsFeaturedTrue()
        {
            await using var context = CreateContext();
            await SeedBaseDataAsync(context);

            context.Properties.Add(
                CreateProperty(1, "Property"));

            await context.SaveChangesAsync();

            var service = new PropertyService(
                context, CreateNotificationMock().Object);

            var result = await service.AddPropertyToFeaturedAsync(1);

            Assert.True(result);

            var property = await context.Properties.FindAsync(1);

            Assert.NotNull(property);
            Assert.True(property.IsFeatured);
        }

        // 7. Add Property To Featured - Not Found
        [Fact]
        public async Task AddPropertyToFeaturedAsync_ShouldThrowArgumentException_WhenPropertyDoesNotExist()
        {
            await using var context = CreateContext();
            await SeedBaseDataAsync(context);

            var service = new PropertyService(
                context, CreateNotificationMock().Object);

            await Assert.ThrowsAsync<ArgumentException>(
                () => service.AddPropertyToFeaturedAsync(999));
        }

        // 8. Add Property To Featured - Not Approved
        [Fact]
        public async Task AddPropertyToFeaturedAsync_ShouldThrowInvalidOperationException_WhenPropertyNotApproved()
        {
            await using var context = CreateContext();
            await SeedBaseDataAsync(context);

            context.Properties.Add(
                CreateProperty(1, "Pending Property",
                    status: PropertyStatus.Pending));

            await context.SaveChangesAsync();

            var service = new PropertyService(
                context, CreateNotificationMock().Object);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.AddPropertyToFeaturedAsync(1));
        }

        // 9. Remove Property From Featured - Success
        [Fact]
        public async Task RemovePropertyFromFeaturedAsync_ShouldSetIsFeaturedFalse()
        {
            await using var context = CreateContext();
            await SeedBaseDataAsync(context);

            context.Properties.Add(
                CreateProperty(1, "Featured Property",
                    isFeatured: true));

            await context.SaveChangesAsync();

            var service = new PropertyService(
                context, CreateNotificationMock().Object);

            var result = await service.RemovePropertyFromFeaturedAsync(1);

            Assert.True(result);

            var property = await context.Properties.FindAsync(1);

            Assert.NotNull(property);
            Assert.False(property.IsFeatured);
        }

        // 10. Remove Property From Featured - Not Found
        [Fact]
        public async Task RemovePropertyFromFeaturedAsync_ShouldThrowArgumentException_WhenPropertyDoesNotExist()
        {
            await using var context = CreateContext();
            await SeedBaseDataAsync(context);

            var service = new PropertyService(
                context, CreateNotificationMock().Object);

            await Assert.ThrowsAsync<ArgumentException>(
                () => service.RemovePropertyFromFeaturedAsync(999));
        }

        // 11. Remove Property From Featured - Not Approved
        [Fact]
        public async Task RemovePropertyFromFeaturedAsync_ShouldThrowInvalidOperationException_WhenPropertyNotApproved()
        {
            await using var context = CreateContext();
            await SeedBaseDataAsync(context);

            context.Properties.Add(
                CreateProperty(1, "Pending Property",
                    status: PropertyStatus.Pending,
                    isFeatured: true));

            await context.SaveChangesAsync();

            var service = new PropertyService(
                context, CreateNotificationMock().Object);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.RemovePropertyFromFeaturedAsync(1));
        }

        // 12. Get My Properties
        [Fact]
        public async Task GetMyPropertiesAsync_ShouldReturnOnlyUserProperties()
        {
            await using var context = CreateContext();
            await SeedBaseDataAsync(context);

            context.Properties.AddRange(
                CreateProperty(1, "User1 Property", userId: "user1"),
                CreateProperty(2, "User2 Property", userId: "user2"),
                CreateProperty(3, "Another User1 Property", userId: "user1"));

            await context.SaveChangesAsync();

            var service = new PropertyService(
                context, CreateNotificationMock().Object);

            var result = await service.GetMyPropertiesAsync("user1");

            var properties = result.ToList();

            Assert.Equal(2, properties.Count);

            Assert.All(properties, p =>
                Assert.Contains(p.Title, new[]
                {
                    "User1 Property",
                    "Another User1 Property"
                }));
        }

        // 13. Search Properties
        [Fact]
        public async Task GetAllPropertiesSearchAsync_ShouldReturnMatchingProperties()
        {
            await using var context = CreateContext();
            await SeedBaseDataAsync(context);

            context.Properties.AddRange(
                CreateProperty(1, "Luxury Apartment"),
                CreateProperty(2, "Small Villa"),
                CreateProperty(3, "Luxury Villa"),
                CreateProperty(4, "Pending Luxury Apartment",
                    status: PropertyStatus.Pending));

            await context.SaveChangesAsync();

            var service = new PropertyService(
                context, CreateNotificationMock().Object);

            var result = await service.GetAllPropertiesSearchAsync("Luxury");

            var properties = result.ToList();

            Assert.Equal(2, properties.Count);

            Assert.All(properties, p =>
                Assert.Contains("Luxury", p.Title));
        }

        // 14. Search Properties - Empty Search
        [Fact]
        public async Task GetAllPropertiesSearchAsync_ShouldReturnEmpty_WhenSearchTermIsEmpty()
        {
            await using var context = CreateContext();
            await SeedBaseDataAsync(context);

            context.Properties.Add(
                CreateProperty(1, "Apartment"));

            await context.SaveChangesAsync();
            var service = new PropertyService(
                context, CreateNotificationMock().Object);

            var result = await service.GetAllPropertiesSearchAsync("   ");

            Assert.Empty(result);
        }

        // 15. Filter Properties - Multiple Filters
        [Fact]
        public async Task FilterPropertiesAsync_ShouldReturnMatchingProperties()
        {
            await using var context = CreateContext();
            await SeedBaseDataAsync(context);

            context.Properties.AddRange(
                CreateProperty(
                    1, "Matching Property",
                    price: 1500000,
                    categoryId: 1,
                    areaId: 1,
                    listingType: ListingType.Sale,
                    bedrooms: 3,
                    bathrooms: 2),

                CreateProperty(
                    2, "High Price Property",
                    price: 5000000),

                CreateProperty(
                    3, "Pending Property",
                    status: PropertyStatus.Pending));

            await context.SaveChangesAsync();

            var service = new PropertyService(
                context, CreateNotificationMock().Object);

            var filter = new PropertyFilterDto
            {
                MinPrice = 1000000,
                MaxPrice = 2000000,
                CityId = 1,
                AreaId = 1,
                CategoryId = 1,
                ListingType = ListingType.Sale,
                Bedrooms = 3,
                Bathrooms = 2
            };

            var result = await service.FilterPropertiesAsync(filter);

            var properties = result.ToList();

            Assert.Single(properties);
            Assert.Equal("Matching Property", properties[0].Title);
        }

        // 16. Get Pending Properties
        [Fact]
        public async Task GetPendingPropertiesAsync_ShouldReturnOnlyPendingProperties()
        {
            await using var context = CreateContext();
            await SeedBaseDataAsync(context);

            context.Properties.AddRange(
                CreateProperty(1, "Pending 1",
                    status: PropertyStatus.Pending),
                CreateProperty(2, "Pending 2",
                    status: PropertyStatus.Pending),
                CreateProperty(3, "Approved",
                    status: PropertyStatus.Approved),
                CreateProperty(4, "Rejected",
                    status: PropertyStatus.Rejected));

            await context.SaveChangesAsync();

            var service = new PropertyService(
                context, CreateNotificationMock().Object);

            var result = await service.GetPendingPropertiesAsync();

            var properties = result.ToList();

            Assert.Equal(2, properties.Count);

            Assert.All(properties, p =>
                Assert.Equal(PropertyStatus.Pending, p.Status));
        }

        // 17. Update Property Status - Approve
        [Fact]
        public async Task UpdatePropertyStatusAsync_ShouldApprovePropertyAndSendNotification()
        {
            await using var context = CreateContext();
            await SeedBaseDataAsync(context);

            context.Properties.Add(
                CreateProperty(1, "Pending Property",
                    userId: "owner1",
                    status: PropertyStatus.Pending));

            await context.SaveChangesAsync();

            var notificationMock = CreateNotificationMock();

            var service = new PropertyService(
                context, notificationMock.Object);

            var result = await service.UpdatePropertyStatusAsync(
                1, PropertyStatus.Approved);

            Assert.True(result);

            var property = await context.Properties.FindAsync(1);

            Assert.NotNull(property);
            Assert.Equal(PropertyStatus.Approved, property.Status);
            Assert.NotNull(property.UpdatedAt);

            notificationMock.Verify(
                n => n.CreateNotificationAsync(
                    "owner1",
                    "Property Approved",
                    It.Is<string>(message =>
                        message.Contains("approved"))),
                Times.Once);
        }

        // 18. Update Property Status - Reject
        [Fact]
        public async Task UpdatePropertyStatusAsync_ShouldRejectPropertyAndSendNotification()
        {
            await using var context = CreateContext();
            await SeedBaseDataAsync(context);

            context.Properties.Add(
                CreateProperty(1, "Pending Property",
                    userId: "owner1",
                    status: PropertyStatus.Pending));

            await context.SaveChangesAsync();

            var notificationMock = CreateNotificationMock();

            var service = new PropertyService(
                context, notificationMock.Object);

            var result = await service.UpdatePropertyStatusAsync(
                1, PropertyStatus.Rejected);

            Assert.True(result);

            var property = await context.Properties.FindAsync(1);

            Assert.NotNull(property);
            Assert.Equal(PropertyStatus.Rejected, property.Status);

            notificationMock.Verify(
                n => n.CreateNotificationAsync(
                    "owner1",
                    "Property Rejected",
                    It.Is<string>(message =>
                        message.Contains("rejected"))),
                Times.Once);
        }

        // 19. Update Property Status - Not Found
        [Fact]
        public async Task UpdatePropertyStatusAsync_ShouldReturnFalse_WhenPropertyDoesNotExist()
        {
            await using var context = CreateContext();
            await SeedBaseDataAsync(context);

            var notificationMock = CreateNotificationMock();

            var service = new PropertyService(
                context, notificationMock.Object);

            var result = await service.UpdatePropertyStatusAsync(
                999, PropertyStatus.Approved);

            Assert.False(result);

            notificationMock.Verify(
                n => n.CreateNotificationAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        // 20. Update Property Status - Invalid Status
        [Fact]
        public async Task UpdatePropertyStatusAsync_ShouldThrowArgumentException_WhenInvalidStatus()
        {
            await using var context = CreateContext();
            await SeedBaseDataAsync(context);

            var service = new PropertyService(
                context, CreateNotificationMock().Object);

            await Assert.ThrowsAsync<ArgumentException>(
                () => service.UpdatePropertyStatusAsync(
                    1, PropertyStatus.Sold));
        }

        // 21. Update Property Status - Property Not Pending
        [Fact]
        public async Task UpdatePropertyStatusAsync_ShouldThrowInvalidOperationException_WhenPropertyIsNotPending()
        {
            await using var context = CreateContext();
            await SeedBaseDataAsync(context);

            context.Properties.Add(
                CreateProperty(1, "Already Approved",
                    status: PropertyStatus.Approved));

            await context.SaveChangesAsync();

            var notificationMock = CreateNotificationMock();

            var service = new PropertyService(
                context, notificationMock.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UpdatePropertyStatusAsync(
                    1, PropertyStatus.Rejected));

            notificationMock.Verify(
                n => n.CreateNotificationAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        // 22. Get Property By Id - Success
        [Fact]
        public async Task GetPropertyByIdAsync_ShouldReturnProperty_WhenApproved()
        {
            await using var context = CreateContext();
            await SeedBaseDataAsync(context);

            context.Properties.Add(
                CreateProperty(1, "Approved Property"));

            await context.SaveChangesAsync();

            var service = new PropertyService(
                context, CreateNotificationMock().Object);

            var result = await service.GetPropertyByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Approved Property", result.Title);
            Assert.Equal("Apartment", result.CategoryName);
            Assert.Equal("Saeed Street", result.AreaName);
            Assert.Equal("Tanta", result.CityName);
        }

        // 23. Get Property By Id - Not Found
        [Fact]
        public async Task GetPropertyByIdAsync_ShouldReturnNull_WhenPropertyDoesNotExist()
        {
            await using var context = CreateContext();
            await SeedBaseDataAsync(context);

            var service = new PropertyService(
                context, CreateNotificationMock().Object);

            var result = await service.GetPropertyByIdAsync(999);

            Assert.Null(result);
        }

        // 24. Get Property By Id - Pending Property
        [Fact]
        public async Task GetPropertyByIdAsync_ShouldReturnNull_WhenPropertyIsNotApproved()
        {
            await using var context = CreateContext();
            await SeedBaseDataAsync(context);

            context.Properties.Add(
                CreateProperty(1, "Pending Property",
                    status: PropertyStatus.Pending));

            await context.SaveChangesAsync();

            var service = new PropertyService(
                context, CreateNotificationMock().Object);

            var result = await service.GetPropertyByIdAsync(1);

            Assert.Null(result);
        }

        // 25. Create Property - Success
        [Fact]
        public async Task CreateAsync_ShouldCreatePropertyWithPendingStatus()
        {
            await using var context = CreateContext();
            await SeedBaseDataAsync(context);

            var service = new PropertyService(
                context, CreateNotificationMock().Object);

            var request = new CreatePropertyDto
            {
                Title = "New Apartment",
                Description = "New apartment description",
                Price = 2000000,
                AreaSize = 150,
                Bedrooms = 3,
                Bathrooms = 2,
                ListingType = ListingType.Sale,
                CategoryId = 1,
                AreaId = 1
            };

            var result = await service.CreateAsync(request, "user1");

            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("New Apartment", result.Title);
            Assert.Equal(2000000, result.Price);
            Assert.Equal(PropertyStatus.Pending, result.Status);
            Assert.Equal("Apartment", result.CategoryName);
            Assert.Equal("Saeed Street", result.AreaName);
            Assert.Equal("Tanta", result.CityName);

            var propertyInDatabase =
                await context.Properties.FindAsync(result.Id);

            Assert.NotNull(propertyInDatabase);
            Assert.Equal("user1", propertyInDatabase.UserId);
            Assert.Equal(PropertyStatus.Pending, propertyInDatabase.Status);
        }

        // 26. Update Property - Success
        [Fact]
        public async Task UpdateAsync_ShouldUpdateProperty_WhenUserOwnsProperty()
        {
            await using var context = CreateContext();
            await SeedBaseDataAsync(context);

            context.Properties.Add(
                CreateProperty(1, "Old Title", userId: "user1"));

            await context.SaveChangesAsync();

            var service = new PropertyService(
                context, CreateNotificationMock().Object);

            var request = new UpdatePropertyDto
            {
                Title = "Updated Title",
                Description = "Updated description",
                Price = 3000000,
                AreaSize = 200,
                Bedrooms = 4,
                Bathrooms = 3,
                ListingType = ListingType.Rent,
                CategoryId = 1,
                AreaId = 1
            };

            var result = await service.UpdateAsync(1, request, "user1");

            Assert.True(result);

            var property = await context.Properties.FindAsync(1);

            Assert.NotNull(property);
            Assert.Equal("Updated Title", property.Title);
            Assert.Equal("Updated description", property.Description);
            Assert.Equal(3000000, property.Price);
            Assert.Equal(200, property.AreaSize);
            Assert.Equal(4, property.Bedrooms);
            Assert.Equal(3, property.Bathrooms);
            Assert.Equal(ListingType.Rent, property.ListingType);
            Assert.Equal(1, property.CategoryId);
            Assert.Equal(1, property.AreaId);
            Assert.NotNull(property.UpdatedAt);
        }

        // 27. Update Property - Not Found
        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenPropertyDoesNotExist()
        {
            await using var context = CreateContext();
            await SeedBaseDataAsync(context);

            var service = new PropertyService(
                context, CreateNotificationMock().Object);

            var request = new UpdatePropertyDto
            {
                Title = "Updated Title",
                Description = "Updated description",
                Price = 3000000,
                AreaSize = 200,
                Bedrooms = 4,
                Bathrooms = 3,
                ListingType = ListingType.Sale,
                CategoryId = 1,
                AreaId = 1
            };

            var result = await service.UpdateAsync(999, request, "user1");

            Assert.False(result);
        }

        // 28. Update Property - Not Owner
        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenUserDoesNotOwnProperty()
        {
            await using var context = CreateContext();
            await SeedBaseDataAsync(context);

            context.Properties.Add(
                CreateProperty(1, "Property", userId: "user1"));

            await context.SaveChangesAsync();

            var service = new PropertyService(
                context, CreateNotificationMock().Object);

            var request = new UpdatePropertyDto
            {
                Title = "Updated Title",
                Description = "Updated description",
                Price = 3000000,
                AreaSize = 200,
                Bedrooms = 4,
                Bathrooms = 3,
                ListingType = ListingType.Sale,
                CategoryId = 1,
                AreaId = 1
            };

            var result = await service.UpdateAsync(1, request, "user2");

            Assert.False(result);

            var property = await context.Properties.FindAsync(1);

            Assert.NotNull(property);
            Assert.Equal("Property", property.Title);
        }

        // 29. Delete Property - Success
        [Fact]
        public async Task DeleteAsync_ShouldDeleteProperty_WhenUserOwnsProperty()
        {
            await using var context = CreateContext();
            await SeedBaseDataAsync(context);

            context.Properties.Add(
                CreateProperty(1, "Property", userId: "user1"));

            await context.SaveChangesAsync();

            var service = new PropertyService(
                context, CreateNotificationMock().Object);

            var result = await service.DeleteAsync(1, "user1");

            Assert.True(result);

            var property = await context.Properties.FindAsync(1);

            Assert.Null(property);
        }

        // 30. Delete Property - Not Found
        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenPropertyDoesNotExist()
        {
            await using var context = CreateContext();
            await SeedBaseDataAsync(context);

            var service = new PropertyService(
                context, CreateNotificationMock().Object);

            var result = await service.DeleteAsync(999, "user1");

            Assert.False(result);
        }

        // 31. Delete Property - Not Owner
        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenUserDoesNotOwnProperty()
        {
            await using var context = CreateContext();
            await SeedBaseDataAsync(context);

            context.Properties.Add(
                CreateProperty(1, "Property", userId: "user1"));

            await context.SaveChangesAsync();

            var service = new PropertyService(
                context, CreateNotificationMock().Object);

            var result = await service.DeleteAsync(1, "user2");

            Assert.False(result);

            Assert.NotNull(await context.Properties.FindAsync(1));
        }
    }
}