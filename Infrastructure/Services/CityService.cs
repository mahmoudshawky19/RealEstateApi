using Application.DTOs.City;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class CityService : ICityService
    {
        private readonly ApplicationDbContext context;
        public CityService(ApplicationDbContext context)
        {
            this.context = context;
        }
        public async Task<IEnumerable<CityResponseDto>> GetAllCitiesAsync()
        {
            var cities = await context.Cities.ToListAsync();
            var cityDtos = cities.Select(c => new CityResponseDto
            {
                Id = c.Id,  
                Name = c.Name,
                CreatedAt = c.CreatedAt
            });
            return cityDtos;
        }

        public async Task<CityResponseDto?> GetCityByIdAsync(int cityId)
        {
            var city = await context.Cities.FindAsync(cityId);
            if (city == null) return null;

            return new CityResponseDto
            {
                Id = city.Id,
                Name = city.Name,
                CreatedAt = city.CreatedAt
            };
        }

        public async Task<CityResponseDto> CreateAsync(CreateCityDto request)
        {
            var city = new City
            {
                Name = request.Name,
                CreatedAt = DateTime.UtcNow,

            };
            context.Cities.Add(city);
            await context.SaveChangesAsync();
            return new CityResponseDto
            {
                Id = city.Id,
                Name = city.Name,
                CreatedAt = city.CreatedAt
            };
        }

        public async Task<bool> UpdateAsync(int id, UpdateCityDto request)
        {
            var city = await context.Cities.FindAsync(id);
            if (city == null) return false;
            city.Name = request.Name;

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var city = await context.Cities.FindAsync(id);
            if (city == null) return false;

            context.Cities.Remove(city);
            await context.SaveChangesAsync();
            return true;
        }
    }
}
