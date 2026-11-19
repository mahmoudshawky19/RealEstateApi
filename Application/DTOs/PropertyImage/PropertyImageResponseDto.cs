using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.PropertyImage
{
    public class PropertyImageResponseDto
    {

        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int PropertyId { get; set; }
        public bool IsPrimary { get; set; }
        public int DisplayOrder { get; set; }
    }
}
