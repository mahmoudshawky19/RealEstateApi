using Application.DTOs.Account;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface IAccountService
    {
        Task<UserProfileDto?> GetMyProfileAsync(string userId);
        Task<IdentityResult> UpdateProfileAsync(
   string userId,
   UpdateProfileDto profileDto);

        Task<IdentityResult> ChangePasswordAsync(
    string userId,
    ChangePasswordDto request);
        Task<bool> ForgotPasswordAsync(string email);
        Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto request);
    }
}
