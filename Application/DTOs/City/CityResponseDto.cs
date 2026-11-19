using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.City
{
    public class CityResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
