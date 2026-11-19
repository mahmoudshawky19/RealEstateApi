using Application.DTOs.City;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface ICityService
    {
        Task<IEnumerable<CityResponseDto>> GetAllCitiesAsync();
        Task<CityResponseDto> GetCityByIdAsync(int cityId);
        Task<CityResponseDto?> CreateAsync(
                CreateCityDto request);
        Task<bool> UpdateAsync(
            int id,
            UpdateCityDto request);
        Task<bool> DeleteAsync(int id);
    }
}
