
using Application.Interfaces;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace RealEstate.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PropertyImagesController : ControllerBase
{
    private readonly IPropertyImageService _imageService;

    public PropertyImagesController(IPropertyImageService imageService)
    {
        _imageService = imageService;
    }

    [HttpGet("{propertyId:int}")]
    public async Task<IActionResult> GetImages(int propertyId)
    {
        var images = await _imageService.GetImagesAsync(propertyId);
        return Ok(images);
    }
    [HttpPost("{propertyId:int}/upload")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(
        int propertyId,
        IFormFile file)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var image = await _imageService.UploadAsync(
                propertyId,
                file,
                userId,
                User.IsInRole(RoleNames.Admin));

            return CreatedAtAction(
                nameof(GetImages),
                new { propertyId },
                image);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    [HttpPut("{imageId:int}/primary")]
    [Authorize]
    public async Task<IActionResult> SetPrimary(int imageId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
            return Unauthorized();

        try
        {
            var result = await _imageService.SetPrimaryAsync(
                imageId,
                userId,
                User.IsInRole(RoleNames.Admin));

            if (!result)
                return NotFound();

            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpDelete("{imageId:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int imageId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
            return Unauthorized();

        try
        {
            var result = await _imageService.DeleteAsync(
                imageId,
                userId,
                User.IsInRole(RoleNames.Admin));

            if (!result)
                return NotFound();

            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
