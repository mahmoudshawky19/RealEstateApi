
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController : ControllerBase
{
    [HttpGet("public")]
    public IActionResult Public()
    {
        return Ok("Anyone can access this endpoint.");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public IActionResult Admin()
    {
        return Ok("Welcome Admin!");
    }
    [Authorize]
    [HttpGet("private")]
    public IActionResult Private()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        var email = User.FindFirstValue(
            ClaimTypes.Email);

        return Ok(new
        {
            Message = "You are authenticated!",
            UserId = userId,
            Email = email
        });
    }
    [HttpGet("test-exception")]
    public IActionResult TestException()
    {
        throw new KeyNotFoundException("Property not found");
    }
}
