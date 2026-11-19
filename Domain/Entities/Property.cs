using Domain.Enums;

namespace Domain.Entities;

public class Property
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal AreaSize { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }

    public ListingType ListingType { get; set; }
    public PropertyStatus Status { get; set; } = PropertyStatus.Pending;
    public bool IsFeatured { get; set; } = false;
    public string UserId { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int AreaId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Category Category { get; set; } = null!;
    public Area Area { get; set; } = null!;

    public ICollection<PropertyImage> Images { get; set; }
        = new List<PropertyImage>();

    public ICollection<Favorite> Favorites { get; set; }
        = new List<Favorite>();

    public ICollection<Inquiry> Inquiries { get; set; }
        = new List<Inquiry>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
