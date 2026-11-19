
using Application.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Tests.Services
{
    public class LocalImageStorageServiceTests
    {
        private readonly Mock<IWebHostEnvironment> _environmentMock;

        public LocalImageStorageServiceTests()
        {
            _environmentMock = new Mock<IWebHostEnvironment>();
        }

        private string CreateTempDirectory()
        {
            var path = Path.Combine(
                Path.GetTempPath(),
                Guid.NewGuid().ToString());

            Directory.CreateDirectory(path);

            return path;
        }

        private IFormFile CreateImageFile(
            string fileName = "image.jpg",
            string content = "Test image content")
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);

            var stream = new MemoryStream(bytes);

            return new FormFile(
                stream,
                0,
                stream.Length,
                "file",
                fileName);
        }

        private LocalImageStorageService CreateService(
            string? webRootPath,
            string contentRootPath)
        {
            _environmentMock
                .Setup(x => x.WebRootPath)
                .Returns(webRootPath);

            _environmentMock
                .Setup(x => x.ContentRootPath)
                .Returns(contentRootPath);

            return new LocalImageStorageService(
                _environmentMock.Object);
        }

        // =========================================
        // UploadAsync
        // =========================================

        [Fact]
        public async Task UploadAsync_ValidFile_ReturnsCorrectUrl()
        {
            // Arrange
            var rootPath = CreateTempDirectory();

            try
            {
                var service = CreateService(
                    rootPath,
                    rootPath);

                var file = CreateImageFile("photo.jpg");

                // Act
                var result = await service.UploadAsync(file);

                // Assert
                Assert.StartsWith(
                    "/uploads/properties/",
                    result);

                Assert.EndsWith(".jpg", result);

                var fileName = Path.GetFileName(result);

                var savedFilePath = Path.Combine(
                    rootPath,
                    "uploads",
                    "properties",
                    fileName);

                Assert.True(File.Exists(savedFilePath));
            }
            finally
            {
                Directory.Delete(rootPath, true);
            }
        }

        [Fact]
        public async Task UploadAsync_ValidFile_SavesCorrectContent()
        {
            // Arrange
            var rootPath = CreateTempDirectory();
            const string expectedContent = "My test image content";

            try
            {
                var service = CreateService(
                    rootPath,
                    rootPath);

                var file = CreateImageFile(
                    "photo.jpg",
                    expectedContent);

                // Act
                var result = await service.UploadAsync(file);

                // Assert
                var fileName = Path.GetFileName(result);

                var savedFilePath = Path.Combine(
                    rootPath,
                    "uploads",
                    "properties",
                    fileName);

                var actualContent = await File.ReadAllTextAsync(
                    savedFilePath);

                Assert.Equal(expectedContent, actualContent);
            }
            finally
            {
                Directory.Delete(rootPath, true);
            }
        }

        [Fact]
        public async Task UploadAsync_UppercaseExtension_ConvertsExtensionToLowercase()
        {
            // Arrange
            var rootPath = CreateTempDirectory();

            try
            {
                var service = CreateService(
                    rootPath,
                    rootPath);

                var file = CreateImageFile("PHOTO.PNG");

                // Act
                var result = await service.UploadAsync(file);

                // Assert
                Assert.EndsWith(".png", result);
            }
            finally
            {
                Directory.Delete(rootPath, true);
            }
        }

        [Fact]
        public async Task UploadAsync_WebRootPathIsNull_UsesContentRootPath()
        {
            // Arrange
            var rootPath = CreateTempDirectory();

            try
            {
                var service = CreateService(
                    null,
                    rootPath);

                var file = CreateImageFile();

                // Act
                var result = await service.UploadAsync(file);

                // Assert
                Assert.StartsWith(
                    "/uploads/properties/",
                    result);

                var fileName = Path.GetFileName(result);

                var savedFilePath = Path.Combine(
                    rootPath,
                    "wwwroot",
                    "uploads",
                    "properties",
                    fileName);

                Assert.True(File.Exists(savedFilePath));
            }
            finally
            {
                Directory.Delete(rootPath, true);
            }
        }

        // =========================================
        // DeleteAsync
        // =========================================

        [Fact]
        public async Task DeleteAsync_FileExists_DeletesFile()
        {
            // Arrange
            var rootPath = CreateTempDirectory();

            try
            {
                var service = CreateService(
                    rootPath,
                    rootPath);

                var uploadsFolder = Path.Combine(
                    rootPath,
                    "uploads",
                    "properties");

                Directory.CreateDirectory(uploadsFolder);

                var fileName = "test-image.jpg";

                var filePath = Path.Combine(
                    uploadsFolder,
                    fileName);

                await File.WriteAllTextAsync(
                    filePath,
                    "Test content");

                var imageUrl = $"/uploads/properties/{fileName}";

                // Act
                await service.DeleteAsync(imageUrl);

                // Assert
                Assert.False(File.Exists(filePath));
            }
            finally
            {
                Directory.Delete(rootPath, true);
            }
        }

        [Fact]
        public async Task DeleteAsync_FileDoesNotExist_CompletesWithoutException()
        {
            // Arrange
            var rootPath = CreateTempDirectory();

            try
            {
                var service = CreateService(
                    rootPath,
                    rootPath);

                var imageUrl =
                    "/uploads/properties/non-existing.jpg";

                // Act
                var exception = await Record.ExceptionAsync(
                    () => service.DeleteAsync(imageUrl));

                // Assert
                Assert.Null(exception);
            }
            finally
            {
                Directory.Delete(rootPath, true);
            }
        }
    }
}
