
using Application.DTOs.PropertyImage;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class PropertyImageService : IPropertyImageService
{
    private readonly ApplicationDbContext _context;
    private readonly IImageStorageService _imageStorage;

    private const long MaxFileSize = 5 * 1024 * 1024;

    public PropertyImageService(
        ApplicationDbContext context,
        IImageStorageService imageStorage)
    {
        _context = context;
        _imageStorage = imageStorage;
    }

    public async Task<IEnumerable<PropertyImageResponseDto>> GetImagesAsync(
        int propertyId)
    {
        var images = await _context.PropertyImages
            .Where(i => i.PropertyId == propertyId)
            .OrderBy(i => i.DisplayOrder)
            .Select(i => new PropertyImageResponseDto
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                IsPrimary = i.IsPrimary,
                DisplayOrder = i.DisplayOrder,
                PropertyId = i.PropertyId
            })
            .ToListAsync();

        return images;
    }

    public async Task<PropertyImageResponseDto> UploadAsync(
        int propertyId,
        IFormFile file,
        string userId,
        bool isAdmin)
    {
        var property = await _context.Properties
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == propertyId);

        if (property == null)
            throw new KeyNotFoundException("Property not found.");

        if (!isAdmin && property.UserId != userId)
            throw new UnauthorizedAccessException("You don't own this property.");

        ValidateImage(file);

        var imageUrl = await _imageStorage.UploadAsync(file);

        try
        {
            var image = new Domain.Entities.PropertyImage
            {
                ImageUrl = imageUrl,
                PropertyId = propertyId,
                IsPrimary = !property.Images.Any(),
                DisplayOrder = property.Images.Count
            };

            _context.PropertyImages.Add(image);

            await _context.SaveChangesAsync();

            return new PropertyImageResponseDto
            {
                Id = image.Id,
                ImageUrl = image.ImageUrl,
                IsPrimary = image.IsPrimary,
                DisplayOrder = image.DisplayOrder,
                PropertyId = image.PropertyId
            };
        }
        catch
        {
            await _imageStorage.DeleteAsync(imageUrl);
            throw;
        }
    }

    public async Task<bool> SetPrimaryAsync(
        int imageId,
        string userId,
        bool isAdmin)
    {
        var image = await _context.PropertyImages
            .Include(i => i.Property)
            .FirstOrDefaultAsync(i => i.Id == imageId);

        if (image == null)
            return false;

        if (!isAdmin && image.Property.UserId != userId)
            throw new UnauthorizedAccessException();

        var propertyImages = await _context.PropertyImages
            .Where(i => i.PropertyId == image.PropertyId)
            .ToListAsync();

        foreach (var item in propertyImages)
        {
            item.IsPrimary = item.Id == imageId;
        }

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(
        int imageId,
        string userId,
        bool isAdmin)
    {
        var image = await _context.PropertyImages
            .Include(i => i.Property)
            .FirstOrDefaultAsync(i => i.Id == imageId);

        if (image == null)
            return false;

        if (!isAdmin && image.Property.UserId != userId)
            throw new UnauthorizedAccessException();

        var propertyId = image.PropertyId;
        var wasPrimary = image.IsPrimary;
        var imageUrl = image.ImageUrl;

        _context.PropertyImages.Remove(image);

        if (wasPrimary)
        {
            var nextImage = await _context.PropertyImages
                .Where(i => i.PropertyId == propertyId &&
                            i.Id != imageId)
                .OrderBy(i => i.DisplayOrder)
                .FirstOrDefaultAsync();

            if (nextImage != null)
                nextImage.IsPrimary = true;
        }

        await _context.SaveChangesAsync();

        await _imageStorage.DeleteAsync(imageUrl);

        return true;
    }

    private static void ValidateImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Please select an image.");

        if (file.Length > MaxFileSize)
            throw new ArgumentException("Maximum image size is 5 MB.");

        var extension = Path.GetExtension(file.FileName)
            .ToLowerInvariant();

        if (extension is not ".jpg" and not ".jpeg"
            and not ".png" and not ".webp")
        {
            throw new ArgumentException("Unsupported image extension.");
        }

        using var stream = file.OpenReadStream();

        var header = new byte[12];
        var bytesRead = stream.Read(header, 0, header.Length);

        var isJpeg = bytesRead >= 3 &&
                     header[0] == 0xFF &&
                     header[1] == 0xD8 &&
                     header[2] == 0xFF;

        var isPng = bytesRead >= 8 &&
                    header.Take(8).SequenceEqual(
                        new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });

        var isWebp = bytesRead >= 12 &&
                     header.Take(4).SequenceEqual(
                         new byte[] { 82, 73, 70, 70 }) &&
                     header.Skip(8).Take(4).SequenceEqual(
                         new byte[] { 87, 69, 66, 80 });

        var validSignature =
            (extension is ".jpg" or ".jpeg") && isJpeg ||
            extension == ".png" && isPng ||
            extension == ".webp" && isWebp;

        if (!validSignature)
            throw new ArgumentException("Invalid image file.");
    }
}
