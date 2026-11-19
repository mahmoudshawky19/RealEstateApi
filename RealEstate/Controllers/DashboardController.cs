
using Application.Interfaces;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RealEstate.Controllers
{
    [Authorize(Roles = RoleNames.Admin)]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview()
        {
            var result = await _dashboardService.GetOverviewAsync();

            return Ok(result);
        }
       
        [HttpGet("recent-properties")]
        public async Task<IActionResult> GetRecentProperties([FromQuery] int count = 5)
        {
            if (count < 1 || count > 50)
                return BadRequest("Count must be between 1 and 50.");

            var result = await _dashboardService.GetRecentPropertiesAsync(count);

            return Ok(result);
        }

        [HttpGet("recent-users")]
        public async Task<IActionResult> GetRecentUsers([FromQuery] int count = 5)
        {
            if (count < 1 || count > 50)
                return BadRequest("Count must be between 1 and 50.");

            var result = await _dashboardService.GetRecentUsersAsync(count);

            return Ok(result);
        }
        [HttpGet("recent-inquiries")]
        public async Task<IActionResult> GetRecentInquiries([FromQuery] int count = 5)
        {
            if (count < 1 || count > 50)
                return BadRequest("Count must be between 1 and 50.");

            var result = await _dashboardService.GetRecentInquiriesAsync(count);

            return Ok(result);
        }
    }
}
