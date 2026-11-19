using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Area;

public class CreateAreaDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int CityId { get; set; }
}