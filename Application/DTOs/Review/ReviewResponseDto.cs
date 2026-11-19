using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Review
{
    public class ReviewResponseDto
    {
        public int Id { get; set; }
         public int PropertyId { get; set; }
        public string PropertyTitle { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
    }
}
