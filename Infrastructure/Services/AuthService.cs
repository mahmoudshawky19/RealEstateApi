
using Application.DTOs.Auth;
using Application.Interfaces;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            AccountType = request.AccountType,
            CompanyName = request.CompanyName
        };
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Failed to create user.");
        }
        var roleResult = await _userManager.AddToRoleAsync(
    user,
    RoleNames.User);

        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ",
                roleResult.Errors.Select(e => e.Description));

            throw new InvalidOperationException(errors);
        }
        var token = await GenerateJwtToken(user);
        return new AuthResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            AccountType = user.AccountType,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1) // Set token expiration time
        };
    }


    private async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        // 1. Get JWT settings
        var jwtSettings = _configuration.GetSection("Jwt");

        var secretKey = jwtSettings["SecretKey"]!;
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];

        // 2. Create Claims
        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Name, user.FullName),
        new Claim(ClaimTypes.Email, user.Email!)
    };

        // 3. Get user roles from Identity
        var roles = await _userManager.GetRolesAsync(user);

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // 4. Create Security Key
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey));

        // 5. Create Signing Credentials
        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        // 6. Create JWT Token
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        // 7. Convert Token to String
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }
        var token = await GenerateJwtToken(user);
        return new AuthResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            AccountType = user.AccountType,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
    }
}
