using Application.DTOs.Property;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Infrastructure.Services
{
    public class PropertyService : IPropertyService
    {
        private readonly ApplicationDbContext context;
        private readonly INotificationService _notificationService;

        public PropertyService(ApplicationDbContext context, INotificationService notificationService)
        {
            this.context = context;
            _notificationService = notificationService;
        }

         private static PropertyResponseDto MapToDto(Property property)
        {
            return new PropertyResponseDto
            {
                Id = property.Id,
                Title = property.Title,
                Description = property.Description,
                Price = property.Price,
                AreaSize = property.AreaSize,
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                ListingType = property.ListingType,
                Status = property.Status,
                CategoryName = property.Category?.Name ?? "",
                IsFeatured = property.IsFeatured,
                AreaName = property.Area?.Name ?? "",
                CityName = property.Area?.City?.Name ?? "",
                CreatedAt = property.CreatedAt,
                PrimaryImageUrl = property.Images?.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                                  ?? property.Images?.FirstOrDefault()?.ImageUrl
            };
        }

        public async Task<IEnumerable<PropertyResponseDto>> GetAllPropertiesAsync()
        {
            var properties = await context.Properties
                .Where(p => p.Status == PropertyStatus.Approved)
                .Include(p => p.Category)
                .Include(p => p.Area).ThenInclude(p => p.City)
                .Include(p => p.Images)
                .ToListAsync();

            return properties.Select(MapToDto);
        }

        public async Task<IEnumerable<PropertyResponseDto>> GetMyPropertiesAsync(string userId)
        {
            var properties = await context.Properties
                .Where(p => p.UserId == userId)
                .Include(p => p.Category)
                .Include(p => p.Area).ThenInclude(p => p.City)
                .Include(p => p.Images)
                .ToListAsync();

            return properties.Select(MapToDto);
        }

        public async Task<IEnumerable<PropertyResponseDto>> GetAllPropertiesFeaturedAsync()
        {
            var properties = await context.Properties
                .Where(p => p.Status == PropertyStatus.Approved && p.IsFeatured)
                .Include(p => p.Category)
                .Include(p => p.Area).ThenInclude(p => p.City)
                .Include(p => p.Images)
                .ToListAsync();

            return properties.Select(MapToDto);
        }

        public async Task<IEnumerable<PropertyResponseDto>> GetRelatedPropertiesAsync(int propertyId)
        {
            var property = await context.Properties
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Area).ThenInclude(p => p.City)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == propertyId);

            if (property == null) throw new ArgumentException("Invalid property id.");

            var relatedProperties = await context.Properties
                .Where(p => p.Status == PropertyStatus.Approved &&
                            p.CategoryId == property.CategoryId &&
                            p.Id != propertyId &&
                            p.ListingType == property.ListingType &&
                            p.Area.CityId == property.Area.CityId)
                .Include(p => p.Category)
                .Include(p => p.Area).ThenInclude(p => p.City)
                .Include(p => p.Images)
                .Take(6)
                .ToListAsync();

            return relatedProperties.Select(MapToDto);
        }

        public async Task<IEnumerable<PropertyResponseDto>> GetPendingPropertiesAsync()
        {
            var properties = await context.Properties
                .Where(p => p.Status == PropertyStatus.Pending)
                .Include(p => p.Category)
                .Include(p => p.Area).ThenInclude(p => p.City)
                .Include(p => p.Images)
                .ToListAsync();

            return properties.Select(MapToDto);
        }

        public async Task<PropertyResponseDto> GetPropertyByIdAsync(int propertyId)
        {
            var property = await context.Properties
                //.Where(p => p.Status == PropertyStatus.Approved)
                .Include(p => p.Category)
                .Include(p => p.Area).ThenInclude(p => p.City)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == propertyId);

            if (property == null) return null;
            return MapToDto(property);
        }

        public async Task<IEnumerable<PropertyResponseDto>> GetAllPropertiesSearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return Enumerable.Empty<PropertyResponseDto>();
            searchTerm = searchTerm.Trim();

            var properties = await context.Properties
                .AsNoTracking()
                .Where(p => p.Status == PropertyStatus.Approved &&
                    (p.Title.Contains(searchTerm) || (p.Description != null && p.Description.Contains(searchTerm))))
                .Include(p => p.Category)
                .Include(p => p.Area).ThenInclude(p => p.City)
                .Include(p => p.Images)
                .ToListAsync();

            return properties.Select(MapToDto);
        }

        public async Task<IEnumerable<PropertyResponseDto>> FilterPropertiesAsync(PropertyFilterDto filter)
        {
            var query = context.Properties
                .Where(p => p.Status == PropertyStatus.Approved)
                .AsQueryable();

            if (filter.MinPrice.HasValue) query = query.Where(p => p.Price >= filter.MinPrice.Value);
            if (filter.MaxPrice.HasValue) query = query.Where(p => p.Price <= filter.MaxPrice.Value);
            if (filter.CityId.HasValue) query = query.Where(p => p.Area.CityId == filter.CityId.Value);
            if (filter.AreaId.HasValue) query = query.Where(p => p.AreaId == filter.AreaId.Value);
            if (filter.CategoryId.HasValue) query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
            if (filter.ListingType.HasValue) query = query.Where(p => p.ListingType == filter.ListingType.Value);
            if (filter.Bedrooms.HasValue) query = query.Where(p => p.Bedrooms == filter.Bedrooms.Value);
            if (filter.Bathrooms.HasValue) query = query.Where(p => p.Bathrooms == filter.Bathrooms.Value);

            var properties = await query
                .Include(p => p.Category)
                .Include(p => p.Area).ThenInclude(p => p.City)
                .Include(p => p.Images)
                .ToListAsync();

            return properties.Select(MapToDto);
        }

        public async Task<IEnumerable<PropertyResponseDto>> SortPropertiesAsync(PropertySortDto sort)
        {
            var query = context.Properties
                .Where(p => p.Status == PropertyStatus.Approved)
                .Include(p => p.Category)
                .Include(p => p.Area).ThenInclude(p => p.City)
                .Include(p => p.Images)
                .AsQueryable();

            query = sort.SortBy switch
            {
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                "oldest" => query.OrderBy(p => p.CreatedAt),
                "priceAsc" => query.OrderBy(p => p.Price),
                "priceDesc" => query.OrderByDescending(p => p.Price),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var properties = await query.ToListAsync();
            return properties.Select(MapToDto);
        }

        public async Task<PropertyResponseDto> CreateAsync(CreatePropertyDto request, string userId)
        {
            var property = new Property
            {
                Title = request.Title,
                Description = request.Description,
                Price = request.Price,
                AreaSize = request.AreaSize,
                Bedrooms = request.Bedrooms,
                Bathrooms = request.Bathrooms,
                ListingType = request.ListingType,
                Status = PropertyStatus.Pending,
                CategoryId = request.CategoryId,
                AreaId = request.AreaId,
                UserId = userId
            };

            context.Properties.Add(property);
            await context.SaveChangesAsync();

             property = await context.Properties
                .Include(p => p.Category)
                .Include(p => p.Area).ThenInclude(p => p.City)
                .Include(p => p.Images)
                .FirstAsync(p => p.Id == property.Id);

            return MapToDto(property);
        }

        public async Task<bool> UpdateAsync(int id, UpdatePropertyDto request, string userId)
        {
            var property = await context.Properties.FindAsync(id);
            if (property == null || property.UserId != userId) return false;

            property.Title = request.Title;
            property.Description = request.Description;
            property.Price = request.Price;
            property.AreaSize = request.AreaSize;
            property.Bedrooms = request.Bedrooms;
            property.Bathrooms = request.Bathrooms;
            property.ListingType = request.ListingType;
            property.UpdatedAt = DateTime.UtcNow;
            property.CategoryId = request.CategoryId;
            property.AreaId = request.AreaId;

            context.Properties.Update(property);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            var property = await context.Properties.FindAsync(id);
            if (property == null || property.UserId != userId) return false;

            context.Properties.Remove(property);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddPropertyToFeaturedAsync(int propertyId)
        {
            var property = await context.Properties.FindAsync(propertyId);
            if (property == null) throw new ArgumentException("Invalid property id.");
            if (property.Status != PropertyStatus.Approved) throw new InvalidOperationException("Only approved properties can be added to featured.");

            property.IsFeatured = true;
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemovePropertyFromFeaturedAsync(int propertyId)
        {
            var property = await context.Properties.FindAsync(propertyId);
            if (property == null) throw new ArgumentException("Invalid property id.");
            if (property.Status != PropertyStatus.Approved) throw new InvalidOperationException("Only approved properties can be added to featured.");

            property.IsFeatured = false;
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePropertyStatusAsync(int id, PropertyStatus status)
        {
            if (status != PropertyStatus.Approved && status != PropertyStatus.Rejected)
                throw new ArgumentException("Invalid property status.");

            var property = await context.Properties.FirstOrDefaultAsync(p => p.Id == id);
            if (property == null) return false;
            if (property.Status != PropertyStatus.Pending) throw new InvalidOperationException("Only pending properties can be reviewed.");

            property.Status = status;
            property.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            var title = status == PropertyStatus.Approved ? "Property Approved" : "Property Rejected";
            var message = status == PropertyStatus.Approved
                ? $"Your property '{property.Title}' has been approved and is now visible to users."
                : $"Your property '{property.Title}' has been rejected.";

            await _notificationService.CreateNotificationAsync(property.UserId, title, message);
            return true;
        }
    }
}