using Application.DTOs.PropertyImage;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces;

public interface IPropertyImageService
{
    Task<IEnumerable<PropertyImageResponseDto>> GetImagesAsync(int propertyId);

    Task<PropertyImageResponseDto> UploadAsync(
        int propertyId,
        IFormFile file,
        string userId,
        bool isAdmin);

    Task<bool> SetPrimaryAsync(
        int imageId,
        string userId,
        bool isAdmin);

    Task<bool> DeleteAsync(
        int imageId,
        string userId,
        bool isAdmin);
}