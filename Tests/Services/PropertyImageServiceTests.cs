
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Tests.Services
{
    public class PropertyImageServiceTests
    {
        private readonly Mock<IImageStorageService> _imageStorageMock;

        public PropertyImageServiceTests()
        {
            _imageStorageMock = new Mock<IImageStorageService>();

            _imageStorageMock
                .Setup(x => x.UploadAsync(It.IsAny<IFormFile>()))
                .ReturnsAsync("/uploads/properties/test-image.jpg");

            _imageStorageMock
                .Setup(x => x.DeleteAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);
        }

        private ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private Property CreateProperty(
            int id = 1,
            string userId = "user-1")
        {
            return new Property
            {
                Id = id,
                Title = $"Property {id}",
                UserId = userId,
                Images = new List<PropertyImage>()
            };
        }

        private PropertyImage CreateImage(
            int id,
            int propertyId = 1,
            string imageUrl = "/uploads/properties/image.jpg",
            bool isPrimary = false,
            int displayOrder = 0,
            Property? property = null)
        {
            return new PropertyImage
            {
                Id = id,
                PropertyId = propertyId,
                ImageUrl = imageUrl,
                IsPrimary = isPrimary,
                DisplayOrder = displayOrder,
                Property = property!
            };
        }

        private IFormFile CreateValidJpegFile(
            string fileName = "image.jpg",
            int size = 100)
        {
            var bytes = new byte[size];

            // JPEG signature
            bytes[0] = 0xFF;
            bytes[1] = 0xD8;
            bytes[2] = 0xFF;

            var stream = new MemoryStream(bytes);

            return new FormFile(
                stream,
                0,
                stream.Length,
                "file",
                fileName);
        }

        private PropertyImageService CreateService(
            ApplicationDbContext context)
        {
            return new PropertyImageService(
                context,
                _imageStorageMock.Object);
        }

        // =========================================
        // GetImagesAsync
        // =========================================

        [Fact]
        public async Task GetImagesAsync_ReturnsPropertyImages()
        {
            // Arrange
            using var context = CreateContext();

            var property = CreateProperty();

            context.Properties.Add(property);

            context.PropertyImages.AddRange(
                CreateImage(
                    1,
                    imageUrl: "image1.jpg",
                    property: property),
                CreateImage(
                    2,
                    imageUrl: "image2.jpg",
                    property: property));

            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            var result = (await service.GetImagesAsync(1)).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("image1.jpg", result[0].ImageUrl);
            Assert.Equal("image2.jpg", result[1].ImageUrl);
        }

        [Fact]
        public async Task GetImagesAsync_ReturnsImagesOrderedByDisplayOrder()
        {
            // Arrange
            using var context = CreateContext();

            var property = CreateProperty();

            context.Properties.Add(property);

            context.PropertyImages.AddRange(
                CreateImage(
                    1,
                    displayOrder: 2,
                    property: property),
                CreateImage(
                    2,
                    displayOrder: 0,
                    property: property),
                CreateImage(
                    3,
                    displayOrder: 1,
                    property: property));

            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            var result = (await service.GetImagesAsync(1)).ToList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal(2, result[0].Id);
            Assert.Equal(3, result[1].Id);
            Assert.Equal(1, result[2].Id);
        }

        [Fact]
        public async Task GetImagesAsync_NoImages_ReturnsEmptyCollection()
        {
            // Arrange
            using var context = CreateContext();
            var service = CreateService(context);

            // Act
            var result = await service.GetImagesAsync(1);

            // Assert
            Assert.Empty(result);
        }

        // =========================================
        // UploadAsync
        // =========================================

        [Fact]
        public async Task UploadAsync_ValidOwner_ReturnsImageResponse()
        {
            // Arrange
            using var context = CreateContext();

            context.Properties.Add(CreateProperty());
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var file = CreateValidJpegFile();

            // Act
            var result = await service.UploadAsync(
                1,
                file,
                "user-1",
                false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.PropertyId);
            Assert.Equal("/uploads/properties/test-image.jpg", result.ImageUrl);
            Assert.True(result.IsPrimary);
            Assert.Equal(0, result.DisplayOrder);

            Assert.Equal(
                1,
                await context.PropertyImages.CountAsync());

            _imageStorageMock.Verify(
                x => x.UploadAsync(file),
                Times.Once);
        }

        [Fact]
        public async Task UploadAsync_AdminCanUploadImage()
        {
            // Arrange
            using var context = CreateContext();

            context.Properties.Add(CreateProperty(userId: "another-user"));
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var file = CreateValidJpegFile();

            // Act
            var result = await service.UploadAsync(
                1,
                file,
                "admin-user",
                true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.PropertyId);

            _imageStorageMock.Verify(
                x => x.UploadAsync(It.IsAny<IFormFile>()),
                Times.Once);
        }

        [Fact]
        public async Task UploadAsync_PropertyDoesNotExist_ThrowsKeyNotFoundException()
        {
            // Arrange
            using var context = CreateContext();

            var service = CreateService(context);
            var file = CreateValidJpegFile();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => service.UploadAsync(
                    999,
                    file,
                    "user-1",
                    false));

            _imageStorageMock.Verify(
                x => x.UploadAsync(It.IsAny<IFormFile>()),
                Times.Never);
        }

        [Fact]
        public async Task UploadAsync_UserDoesNotOwnProperty_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            using var context = CreateContext();

            context.Properties.Add(
                CreateProperty(userId: "another-user"));

            await context.SaveChangesAsync();

            var service = CreateService(context);
            var file = CreateValidJpegFile();

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.UploadAsync(
                    1,
                    file,
                    "user-1",
                    false));

            _imageStorageMock.Verify(
                x => x.UploadAsync(It.IsAny<IFormFile>()),
                Times.Never);
        }

        [Fact]
        public async Task UploadAsync_SecondImage_IsNotPrimaryAndHasNextDisplayOrder()
        {
            // Arrange
            using var context = CreateContext();

            var property = CreateProperty();

            context.Properties.Add(property);

            context.PropertyImages.Add(
                CreateImage(
                    1,
                    property: property,
                    isPrimary: true,
                    displayOrder: 0));

            await context.SaveChangesAsync();

            _imageStorageMock
                .Setup(x => x.UploadAsync(It.IsAny<IFormFile>()))
                .ReturnsAsync("/uploads/properties/second.jpg");

            var service = CreateService(context);
            var file = CreateValidJpegFile();

            // Act
            var result = await service.UploadAsync(
                1,
                file,
                "user-1",
                false);

            // Assert
            Assert.False(result.IsPrimary);
            Assert.Equal(1, result.DisplayOrder);

            var images = await context.PropertyImages
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            Assert.Equal(2, images.Count);
            Assert.True(images[0].IsPrimary);
            Assert.False(images[1].IsPrimary);
        }

        [Fact]
        public async Task UploadAsync_EmptyFile_ThrowsArgumentException()
        {
            // Arrange
            using var context = CreateContext();

            context.Properties.Add(CreateProperty());
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var emptyStream = new MemoryStream();

            var file = new FormFile(
                emptyStream,
                0,
                0,
                "file",
                "empty.jpg");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => service.UploadAsync(
                    1,
                    file,
                    "user-1",
                    false));

            Assert.Equal(
                "Please select an image.",
                exception.Message);

            _imageStorageMock.Verify(
                x => x.UploadAsync(It.IsAny<IFormFile>()),
                Times.Never);
        }

        [Fact]
        public async Task UploadAsync_FileExceedsMaximumSize_ThrowsArgumentException()
        {
            // Arrange
            using var context = CreateContext();

            context.Properties.Add(CreateProperty());
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var file = CreateValidJpegFile(
                size: 5 * 1024 * 1024 + 1);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => service.UploadAsync(
                    1,
                    file,
                    "user-1",
                    false));

            Assert.Equal(
                "Maximum image size is 5 MB.",
                exception.Message);

            _imageStorageMock.Verify(
                x => x.UploadAsync(It.IsAny<IFormFile>()),
                Times.Never);
        }

        [Fact]
        public async Task UploadAsync_UnsupportedExtension_ThrowsArgumentException()
        {
            // Arrange
            using var context = CreateContext();

            context.Properties.Add(CreateProperty());
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var file = CreateValidJpegFile("document.gif");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => service.UploadAsync(
                    1,
                    file,
                    "user-1",
                    false));

            Assert.Equal(
                "Unsupported image extension.",
                exception.Message);
        }

        [Fact]
        public async Task UploadAsync_InvalidImageSignature_ThrowsArgumentException()
        {
            // Arrange
            using var context = CreateContext();

            context.Properties.Add(CreateProperty());
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var invalidBytes = new byte[100];
            var stream = new MemoryStream(invalidBytes);

            var file = new FormFile(
                stream,
                0,
                stream.Length,
                "file",
                "fake.jpg");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => service.UploadAsync(
                    1,
                    file,
                    "user-1",
                    false));

            Assert.Equal(
                "Invalid image file.",
                exception.Message);

            _imageStorageMock.Verify(
                x => x.UploadAsync(It.IsAny<IFormFile>()),
                Times.Never);
        }

        // =========================================
        // SetPrimaryAsync
        // =========================================

        [Fact]
        public async Task SetPrimaryAsync_ImageExists_SetsItAsPrimary()
        {
            // Arrange
            using var context = CreateContext();

            var property = CreateProperty();

            context.Properties.Add(property);

            context.PropertyImages.AddRange(
                CreateImage(1, property: property, isPrimary: true),
                CreateImage(2, property: property));

            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            var result = await service.SetPrimaryAsync(
                2,
                "user-1",
                false);

            // Assert
            Assert.True(result);

            var images = await context.PropertyImages
                .AsNoTracking()
                .OrderBy(i => i.Id)
                .ToListAsync();

            Assert.False(images[0].IsPrimary);
            Assert.True(images[1].IsPrimary);
        }

        [Fact]
        public async Task SetPrimaryAsync_ImageDoesNotExist_ReturnsFalse()
        {
            // Arrange
            using var context = CreateContext();
            var service = CreateService(context);

            // Act
            var result = await service.SetPrimaryAsync(
                999,
                "user-1",
                false);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task SetPrimaryAsync_UserDoesNotOwnProperty_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            using var context = CreateContext();

            var property = CreateProperty(userId: "another-user");

            context.Properties.Add(property);

            context.PropertyImages.Add(
                CreateImage(1, property: property));

            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.SetPrimaryAsync(
                    1,
                    "user-1",
                    false));
        }

        // =========================================
        // DeleteAsync
        // =========================================

        [Fact]
        public async Task DeleteAsync_ImageExists_DeletesImageAndStorageFile()
        {
            // Arrange
            using var context = CreateContext();

            var property = CreateProperty();

            context.Properties.Add(property);

            context.PropertyImages.Add(
                CreateImage(
                    1,
                    imageUrl: "/uploads/properties/image.jpg",
                    property: property));

            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            var result = await service.DeleteAsync(
                1,
                "user-1",
                false);

            // Assert
            Assert.True(result);

            Assert.False(
                await context.PropertyImages.AnyAsync(i => i.Id == 1));

            _imageStorageMock.Verify(
                x => x.DeleteAsync("/uploads/properties/image.jpg"),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ImageDoesNotExist_ReturnsFalse()
        {
            // Arrange
            using var context = CreateContext();
            var service = CreateService(context);

            // Act
            var result = await service.DeleteAsync(
                999,
                "user-1",
                false);

            // Assert
            Assert.False(result);

            _imageStorageMock.Verify(
                x => x.DeleteAsync(It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_UserDoesNotOwnProperty_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            using var context = CreateContext();

            var property = CreateProperty(userId: "another-user");

            context.Properties.Add(property);

            context.PropertyImages.Add(
                CreateImage(1, property: property));

            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.DeleteAsync(
                    1,
                    "user-1",
                    false));

            Assert.True(
                await context.PropertyImages.AnyAsync(i => i.Id == 1));

            _imageStorageMock.Verify(
                x => x.DeleteAsync(It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_DeletingPrimaryImage_SetsNextImageAsPrimary()
        {
            // Arrange
            using var context = CreateContext();

            var property = CreateProperty();

            context.Properties.Add(property);

            context.PropertyImages.AddRange(
                CreateImage(
                    1,
                    imageUrl: "first.jpg",
                    property: property,
                    isPrimary: true,
                    displayOrder: 0),
                CreateImage(
                    2,
                    imageUrl: "second.jpg",
                    property: property,
                    displayOrder: 1),
                CreateImage(
                    3,
                    imageUrl: "third.jpg",
                    property: property,
                    displayOrder: 2));

            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            var result = await service.DeleteAsync(
                1,
                "user-1",
                false);

            // Assert
            Assert.True(result);

            var images = await context.PropertyImages
                .AsNoTracking()
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            Assert.Equal(2, images.Count);
            Assert.Equal(2, images[0].Id);
            Assert.True(images[0].IsPrimary);
            Assert.False(images[1].IsPrimary);

            _imageStorageMock.Verify(
                x => x.DeleteAsync("first.jpg"),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_AdminCanDeleteImage()
        {
            // Arrange
            using var context = CreateContext();

            var property = CreateProperty(userId: "another-user");

            context.Properties.Add(property);

            context.PropertyImages.Add(
                CreateImage(1, property: property));

            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            var result = await service.DeleteAsync(
                1,
                "admin-user",
                true);

            // Assert
            Assert.True(result);

            Assert.False(
                await context.PropertyImages.AnyAsync(i => i.Id == 1));
        }
    }
}
