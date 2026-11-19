using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Review;

public class CreateReviewDto
{
    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }

    [Range(1, int.MaxValue)]
    public int PropertyId { get; set; }
}