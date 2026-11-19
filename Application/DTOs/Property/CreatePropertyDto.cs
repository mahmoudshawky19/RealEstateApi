
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Property;

public class CreatePropertyDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Range(1, int.MaxValue)]
    public int AreaId { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal AreaSize { get; set; }

    public int? Bedrooms { get; set; }

    public int? Bathrooms { get; set; }

    public ListingType ListingType { get; set; }
}
