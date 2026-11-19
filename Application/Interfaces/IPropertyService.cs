using Application.DTOs.Property;
using Domain.Enums;

namespace Application.Interfaces
{
    public interface IPropertyService
    {
        Task<IEnumerable<PropertyResponseDto>> GetAllPropertiesAsync();
        Task<IEnumerable<PropertyResponseDto>> GetAllPropertiesSearchAsync(string searchTerm);
        Task<IEnumerable<PropertyResponseDto>> GetMyPropertiesAsync(string userId);
        Task<IEnumerable<PropertyResponseDto>> FilterPropertiesAsync(PropertyFilterDto filter);
        Task<PropertyResponseDto> GetPropertyByIdAsync(int propertyId);
        Task<IEnumerable<PropertyResponseDto>> SortPropertiesAsync(PropertySortDto sort);
        Task<IEnumerable<PropertyResponseDto>> GetAllPropertiesFeaturedAsync();
        Task<IEnumerable<PropertyResponseDto>> GetRelatedPropertiesAsync(int propertyId);
        Task<bool> AddPropertyToFeaturedAsync(int propertyId);

        Task<bool> RemovePropertyFromFeaturedAsync(int propertyId);

        Task<PropertyResponseDto> CreateAsync(
                CreatePropertyDto request,
                string userId);
        
        Task<bool> UpdatePropertyStatusAsync(int id, PropertyStatus status);
        Task<IEnumerable<PropertyResponseDto>> GetPendingPropertiesAsync();
        Task<bool> UpdateAsync(
            int id,
            UpdatePropertyDto request,
            string userId);

        Task<bool> DeleteAsync(
            int id,
            string userId);
    }
}
