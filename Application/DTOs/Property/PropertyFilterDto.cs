using Domain.Enums;

namespace Application.DTOs.Property;

public class PropertyFilterDto
{
    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public int? CityId { get; set; }

    public int? AreaId { get; set; }

    public int? CategoryId { get; set; }

    public ListingType? ListingType { get; set; }

    public int? Bedrooms { get; set; }

    public int? Bathrooms { get; set; }
}