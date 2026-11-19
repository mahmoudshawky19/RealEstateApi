using Application.DTOs.Account;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace RealEstate.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }
        [HttpGet("profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();
            var profile = await _accountService.GetMyProfileAsync(userId);

            if (profile == null)
                return NotFound();

            return Ok(profile);
        }
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(
       [FromBody] UpdateProfileDto profileDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var result = await _accountService.UpdateProfileAsync(userId, profileDto);

            if (!result.Succeeded)
            {
                if (result.Errors.Any(e => e.Description == "User not found."))
                    return NotFound();

                return BadRequest(new
                {
                    errors = result.Errors.Select(e => e.Description)
                });
            }

            return NoContent();
        }
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var result = await _accountService.ChangePasswordAsync(userId, request);

            if (!result.Succeeded)
            {
                if (result.Errors.Any(e => e.Description == "User not found."))
                    return NotFound();

                return BadRequest(new
                {
                    errors = result.Errors.Select(e => e.Description)
                });
            }

            return NoContent();
        }
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(
    [FromBody] ForgotPasswordDto request)
        {
            await _accountService.ForgotPasswordAsync(request.Email);

            return Ok(new
            {
                message = "If the email exists, password reset instructions will be sent."
            });
        }
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
            var result = await _accountService.ResetPasswordAsync(request);

            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    errors = result.Errors.Select(e => e.Description)
                });
            }

            return Ok(new
            {
                message = "Password has been reset successfully."
            });
        }
    }
}
