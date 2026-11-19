using Application.DTOs.Area;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Services
{
    public class AreaService :IAreaService
    {
        private readonly ApplicationDbContext context;
        public AreaService(ApplicationDbContext context)
        {
            this.context = context;
        }
        public async Task<IEnumerable<AreaResponseDto>> GetAllAreasAsync()
        {
            var areas = await context.Areas.Include(a => a.City).ToListAsync();
           return areas.Select(a => new AreaResponseDto
           {
               Id = a.Id,
               Name = a.Name,
               CityId = a.CityId,
               CityName = a.City.Name
           });
        }
        public async Task<AreaResponseDto?> GetAreaByIdAsync(int areaId)
        {
            var area = await context.Areas.Include(a => a.City).FirstOrDefaultAsync(a => a.Id == areaId);
            if (area == null)
            {
                return null;
            }
            return new AreaResponseDto
            {
                Id = area.Id,
                Name = area.Name,
                CityId = area.CityId,
                CityName = area.City.Name
            };
        }
        public async Task<AreaResponseDto> CreateAsync(CreateAreaDto createAreaDto)
        {
            var area = new Domain.Entities.Area
            {
                Name = createAreaDto.Name,
                CityId = createAreaDto.CityId
            };
            var city = await context.Cities.FindAsync(createAreaDto.CityId);

            if (city == null)
                throw new KeyNotFoundException("City not found.");
            context.Areas.Add(area);
            await context.SaveChangesAsync();
            return new AreaResponseDto
            {
                Id = area.Id,
                Name = area.Name,
                CityId = area.CityId,
                CityName = city.Name
            };
        }
        public async Task<AreaResponseDto?> UpdateAsync(int id, UpdateAreaDto updateAreaDto)
        {
            var area = await context.Areas.FindAsync(id);
            if (area == null)
            {
                return null ;
            }
            area.Name = updateAreaDto.Name;
            var city = await context.Cities.FindAsync(updateAreaDto.CityId);

            if (city == null)
                throw new KeyNotFoundException("City not found.");
            area.CityId = updateAreaDto.CityId;
            await context.SaveChangesAsync();
            return new AreaResponseDto
            {
                Id = area.Id,
                Name = area.Name,
                CityId = area.CityId,
                CityName = area.City.Name
            }   ;
        }
        public async Task<bool> DeleteAsync(int areaId)
        {
            var area = await context.Areas.FindAsync(areaId);
            if (area == null)
            {
                return false;
            }
            context.Areas.Remove(area);
            await context.SaveChangesAsync();
            return true;
        }
    }
}
