using Application.DTOs.Area;
using Application.Interfaces;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RealEstate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AreasController : ControllerBase
    {
        private readonly IAreaService _areaService;
        public AreasController(IAreaService areaService)
        {
            _areaService = areaService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AreaResponseDto>>> GetAllAreas()
        {
            var areas = await _areaService.GetAllAreasAsync();
            return Ok(areas);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<AreaResponseDto>> GetAreaById(int id)
        {
            var area = await _areaService.GetAreaByIdAsync(id);
            if (area == null)
            {
                return NotFound();
            }
            return Ok(area);
        }
        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<AreaResponseDto>> CreateArea([FromBody] CreateAreaDto createAreaDto)
        {
            var area = await _areaService.CreateAsync(createAreaDto);
            return CreatedAtAction(nameof(GetAreaById), new { id = area.Id }, area);
        }
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<AreaResponseDto>> UpdateArea(int id, [FromBody] UpdateAreaDto updateAreaDto)
        {
            var area = await _areaService.UpdateAsync(id, updateAreaDto);
            if (area == null)
            {
                return NotFound();
            }
            return Ok(area);
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<bool>> DeleteArea(int id)
        {
            var result = await _areaService.DeleteAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return Ok(result);
        }
    }
}
