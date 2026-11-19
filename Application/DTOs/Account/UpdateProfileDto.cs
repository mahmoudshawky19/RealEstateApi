using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Account;

public class UpdateProfileDto
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [MaxLength(150)]
    public string? CompanyName { get; set; }
}