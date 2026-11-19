using Application.DTOs.Account;
using Application.Interfaces;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public AccountService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task<UserProfileDto?> GetMyProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return null;

            return new UserProfileDto
            {
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                AccountType = user.AccountType,
                CompanyName = user.CompanyName,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
        public async Task<IdentityResult> UpdateProfileAsync(
          string userId,
          UpdateProfileDto profileDto)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return IdentityResult.Failed(
                    new IdentityError { Description = "User not found." });

            user.FullName = profileDto.FullName;
            user.PhoneNumber = profileDto.PhoneNumber;
            user.CompanyName = profileDto.CompanyName;
            user.UpdatedAt = DateTime.UtcNow;

            return await _userManager.UpdateAsync(user);
        }
        public async Task<IdentityResult> ChangePasswordAsync(
        string userId,
        ChangePasswordDto request)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return IdentityResult.Failed(
                    new IdentityError
                    {
                        Description = "User not found."
                    });
            }

            if (request.CurrentPassword == request.NewPassword)
            {
                return IdentityResult.Failed(
                    new IdentityError
                    {
                        Description = "New password cannot be the same as the current password."
                    });
            }

            return await _userManager.ChangePasswordAsync(
                user,
                request.CurrentPassword,
                request.NewPassword);
        }
        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return false;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // مؤقتًا أثناء التطوير
            // هنستخدم الـ Token لاختبار Reset Password
            Console.WriteLine($"Password Reset Token: {token}");

            return true;
        }
        public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return IdentityResult.Failed(
                    new IdentityError
                    {
                        Description = "User not found."
                    });
            }
           
            return await _userManager.ResetPasswordAsync(
                user,
                request.Token,
                request.NewPassword);
        }
    }
}
