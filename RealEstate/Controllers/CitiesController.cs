using Application.DTOs.City;
using Application.Interfaces;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RealEstate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CitiesController : ControllerBase
    {
        private readonly ICityService _cityService;
        public CitiesController(ICityService cityService)
        {
            _cityService = cityService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllCities()
        {
            var cities = await _cityService.GetAllCitiesAsync();
            return Ok(cities);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCityById(int id)
        {
            var city = await _cityService.GetCityByIdAsync(id);
            if (city == null) return NotFound();
            return Ok(city);
        }
        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<IActionResult> CreateCity([FromBody] CreateCityDto request)
        {
            var city = await _cityService.CreateAsync(request);
            return Ok(city);
        }
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Admin)]

        public async Task<IActionResult> UpdateCity(int id, [FromBody] UpdateCityDto request)
        {
            var result = await _cityService.UpdateAsync(id, request);
            if (!result) return NotFound();
            return Ok();
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Admin)]

        public async Task<IActionResult> DeleteCity(int id)
        {
            var result = await _cityService.DeleteAsync(id);
            if (!result) return NotFound();
            return Ok();
        }
    }
}
