using Application.DTOs.Property;
using Application.Interfaces;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace RealEstate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropertiesController : ControllerBase
    {
        private readonly IPropertyService propertyService;
        public PropertiesController(IPropertyService propertyService)
        {
            this.propertyService = propertyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProperties()
        {
            var properties = await propertyService.GetAllPropertiesAsync();
            return Ok(properties);
        }
        [HttpGet("my-properties")]
        [Authorize]
        public async Task<IActionResult> GetMyProperties()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();
            var properties = await propertyService.GetMyPropertiesAsync(userId);
            return Ok(properties);
        }
        [HttpGet("featured")]
        public async Task<IActionResult> GetAllPropertiesFeatured()
        {
            var properties = await propertyService.GetAllPropertiesFeaturedAsync();
            return Ok(properties);
        }
        [HttpPut("{id}/Featured")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<IActionResult> AddPropertyToFeatured(int id)
        {
            try
            {
              var result = await propertyService.AddPropertyToFeaturedAsync(id);
                if (!result)
                    return NotFound(new { message = "Property not found." });

                return NoContent();
                    }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("{id}/Featured")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<IActionResult> RemovePropertyFromFeatured(int id)
        {
            try
            {
              var result = await propertyService.RemovePropertyFromFeaturedAsync(id);
                if (!result)
                    return NotFound(new { message = "Property not found." });

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetAllPropertiesSearch([FromQuery] string searchTerm)
        {
            var properties = await propertyService.GetAllPropertiesSearchAsync(searchTerm);
            return Ok(properties);
        }
        [HttpGet("filter")]
        public async Task<IActionResult> FilterProperties([FromQuery] PropertyFilterDto filter)
        {
            var properties = await propertyService.FilterPropertiesAsync(filter);
            return Ok(properties);
        }
        [HttpGet("sort")]
        public async Task<IActionResult> SortProperties([FromQuery] PropertySortDto sort)
        {
            var properties = await propertyService.SortPropertiesAsync(sort);
            return Ok(properties);
        }
        [HttpGet("{id:int}/related")]
        public async Task<IActionResult> GetRelatedProperties(int id)
        {
            try
            {
                var properties = await propertyService.GetRelatedPropertiesAsync(id);
                return Ok(properties);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        [HttpGet("pending")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<IActionResult> GetAllPropertiesPending()
        {
            var properties = await propertyService.GetPendingPropertiesAsync();
            return Ok(properties);
        }
        [HttpPut("{id:int}/status")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<IActionResult> UpdatePropertyStatus(
      int id,
      [FromBody] UpdatePropertyStatusDto request)
        {
            try
            {
                var result = await propertyService.UpdatePropertyStatusAsync(
                    id, request.Status);

                if (!result)
                    return NotFound(new { message = "Property not found." });

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPropertyById(int id)
        {
            var property = await propertyService.GetPropertyByIdAsync(id);
            if (property == null)
            {
                return NotFound();
            }
            return Ok(property);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProperty([FromBody] CreatePropertyDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();
            var property = await propertyService.CreateAsync(request, userId);
            return Ok(property);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProperty(int id, [FromBody] UpdatePropertyDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();
            bool property = await propertyService.UpdateAsync(id, request, userId);
            if (!property)
            {
                return NotFound();
            }
            return Ok("Property updated successfully");
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProperty(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();
            bool result = await propertyService.DeleteAsync(id, userId);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
