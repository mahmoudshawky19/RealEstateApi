using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Inquiry;

public class CreateInquiryDto
{
    [Required]
    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int PropertyId { get; set; }
}