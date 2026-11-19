using Application.DTOs.Area;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface IAreaService
    {
        Task<IEnumerable<AreaResponseDto>> GetAllAreasAsync();

        Task<AreaResponseDto?> GetAreaByIdAsync(int areaId);

        Task<AreaResponseDto> CreateAsync(CreateAreaDto request);

        Task<AreaResponseDto?> UpdateAsync(int id, UpdateAreaDto request);

        Task<bool> DeleteAsync(int id);
    }
}
