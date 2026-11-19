namespace Application.DTOs.Favorite;

public class FavoriteResponseDto
{
    public int PropertyId { get; set; }
    public string PropertyTitle { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}