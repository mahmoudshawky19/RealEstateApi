using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.DTOs.Category
{
    public class UpdateCategoryDto
    {
      
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(500)]

        public string? Description { get; set; }
    }
}
