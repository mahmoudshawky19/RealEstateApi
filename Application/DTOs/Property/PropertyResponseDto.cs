
using Domain.Enums;

namespace Application.DTOs.Property;

public class PropertyResponseDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public decimal AreaSize { get; set; }

    public int? Bedrooms { get; set; }

    public int? Bathrooms { get; set; }
    public string? PrimaryImageUrl { get; set; }

    public ListingType ListingType { get; set; }

    public PropertyStatus Status { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string AreaName { get; set; } = string.Empty;

    public string CityName { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; }
}
