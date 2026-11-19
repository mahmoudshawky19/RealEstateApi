using Application.DTOs.Account;
using Application.Interfaces;
using Infrastructure.Identity;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tests.Services
{
    public class AccountServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly AccountService _accountService;

        public AccountServiceTests()
        {
            _userManagerMock = CreateUserManagerMock();
            _accountService = new AccountService(_userManagerMock.Object);
        }

        private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();

            return new Mock<UserManager<ApplicationUser>>(
                store.Object,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!);
        }

        private ApplicationUser CreateUser(
            string id = "user-1",
            string email = "test@example.com")
        {
            return new ApplicationUser
            {
                Id = id,
                FullName = "Test User",
                Email = email,
                PhoneNumber = "01012345678",
                CompanyName = "Test Company",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };
        }

        // =========================================
        // GetMyProfileAsync
        // =========================================

        [Fact]
        public async Task GetMyProfileAsync_UserExists_ReturnsProfile()
        {
            // Arrange
            var user = CreateUser();

            _userManagerMock
                .Setup(x => x.FindByIdAsync("user-1"))
                .ReturnsAsync(user);

            // Act
            var result = await _accountService.GetMyProfileAsync("user-1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.FullName, result.FullName);
            Assert.Equal(user.Email, result.Email);
            Assert.Equal(user.PhoneNumber, result.PhoneNumber);
            Assert.Equal(user.AccountType, result.AccountType);
            Assert.Equal(user.CompanyName, result.CompanyName);
            Assert.Equal(user.CreatedAt, result.CreatedAt);
            Assert.Equal(user.UpdatedAt, result.UpdatedAt);

            _userManagerMock.Verify(
                x => x.FindByIdAsync("user-1"),
                Times.Once);
        }

        [Fact]
        public async Task GetMyProfileAsync_UserDoesNotExist_ReturnsNull()
        {
            // Arrange
            _userManagerMock
                .Setup(x => x.FindByIdAsync("wrong-id"))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _accountService.GetMyProfileAsync("wrong-id");

            // Assert
            Assert.Null(result);
        }

        // =========================================
        // UpdateProfileAsync
        // =========================================

        [Fact]
        public async Task UpdateProfileAsync_UserExists_UpdatesUserSuccessfully()
        {
            // Arrange
            var user = CreateUser();

            var request = new UpdateProfileDto
            {
                FullName = "Updated Name",
                PhoneNumber = "01111111111",
                CompanyName = "Updated Company"
            };

            _userManagerMock
                .Setup(x => x.FindByIdAsync("user-1"))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _accountService.UpdateProfileAsync(
                "user-1",
                request);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("Updated Name", user.FullName);
            Assert.Equal("01111111111", user.PhoneNumber);
            Assert.Equal("Updated Company", user.CompanyName);
            Assert.NotNull(user.UpdatedAt);

            _userManagerMock.Verify(
                x => x.UpdateAsync(user),
                Times.Once);
        }

        [Fact]
        public async Task UpdateProfileAsync_UserDoesNotExist_ReturnsFailedResult()
        {
            // Arrange
            var request = new UpdateProfileDto
            {
                FullName = "Updated Name",
                PhoneNumber = "01111111111",
                CompanyName = "Updated Company"
            };

            _userManagerMock
                .Setup(x => x.FindByIdAsync("wrong-id"))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _accountService.UpdateProfileAsync(
                "wrong-id",
                request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains(
                result.Errors,
                e => e.Description == "User not found.");

            _userManagerMock.Verify(
                x => x.UpdateAsync(It.IsAny<ApplicationUser>()),
                Times.Never);
        }

        // =========================================
        // ChangePasswordAsync
        // =========================================

        [Fact]
        public async Task ChangePasswordAsync_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var user = CreateUser();

            var request = new ChangePasswordDto
            {
                CurrentPassword = "OldPassword123!",
                NewPassword = "NewPassword123!"
            };

            _userManagerMock
                .Setup(x => x.FindByIdAsync("user-1"))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.ChangePasswordAsync(
                    user,
                    request.CurrentPassword,
                    request.NewPassword))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _accountService.ChangePasswordAsync(
                "user-1",
                request);

            // Assert
            Assert.True(result.Succeeded);

            _userManagerMock.Verify(
                x => x.ChangePasswordAsync(
                    user,
                    "OldPassword123!",
                    "NewPassword123!"),
                Times.Once);
        }

        [Fact]
        public async Task ChangePasswordAsync_UserDoesNotExist_ReturnsFailedResult()
        {
            // Arrange
            var request = new ChangePasswordDto
            {
                CurrentPassword = "OldPassword123!",
                NewPassword = "NewPassword123!"
            };

            _userManagerMock
                .Setup(x => x.FindByIdAsync("wrong-id"))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _accountService.ChangePasswordAsync(
                "wrong-id",
                request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains(
                result.Errors,
                e => e.Description == "User not found.");

            _userManagerMock.Verify(
                x => x.ChangePasswordAsync(
                    It.IsAny<ApplicationUser>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task ChangePasswordAsync_NewPasswordSameAsCurrent_ReturnsFailedResult()
        {
            // Arrange
            var user = CreateUser();

            var request = new ChangePasswordDto
            {
                CurrentPassword = "SamePassword123!",
                NewPassword = "SamePassword123!"
            };

            _userManagerMock
                .Setup(x => x.FindByIdAsync("user-1"))
                .ReturnsAsync(user);

            // Act
            var result = await _accountService.ChangePasswordAsync(
                "user-1",
                request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains(
                result.Errors,
                e => e.Description ==
                "New password cannot be the same as the current password.");

            _userManagerMock.Verify(
                x => x.ChangePasswordAsync(
                    It.IsAny<ApplicationUser>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task ChangePasswordAsync_IdentityRejectsPassword_ReturnsFailedResult()
        {
            // Arrange
            var user = CreateUser();

            var request = new ChangePasswordDto
            {
                CurrentPassword = "OldPassword123!",
                NewPassword = "weak"
            };

            var identityResult = IdentityResult.Failed(
                new IdentityError
                {
                    Description = "Password is too weak."
                });

            _userManagerMock
                .Setup(x => x.FindByIdAsync("user-1"))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.ChangePasswordAsync(
                    user,
                    request.CurrentPassword,
                    request.NewPassword))
                .ReturnsAsync(identityResult);

            // Act
            var result = await _accountService.ChangePasswordAsync(
                "user-1",
                request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains(
                result.Errors,
                e => e.Description == "Password is too weak.");
        }

        // =========================================
        // ForgotPasswordAsync
        // =========================================

        [Fact]
        public async Task ForgotPasswordAsync_UserExists_ReturnsTrue()
        {
            // Arrange
            var user = CreateUser();

            _userManagerMock
                .Setup(x => x.FindByEmailAsync("test@example.com"))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.GeneratePasswordResetTokenAsync(user))
                .ReturnsAsync("reset-token-123");

            // Act
            var result = await _accountService.ForgotPasswordAsync(
                "test@example.com");

            // Assert
            Assert.True(result);

            _userManagerMock.Verify(
                x => x.GeneratePasswordResetTokenAsync(user),
                Times.Once);
        }

        [Fact]
        public async Task ForgotPasswordAsync_UserDoesNotExist_ReturnsFalse()
        {
            // Arrange
            _userManagerMock
                .Setup(x => x.FindByEmailAsync("unknown@example.com"))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _accountService.ForgotPasswordAsync(
                "unknown@example.com");

            // Assert
            Assert.False(result);

            _userManagerMock.Verify(
                x => x.GeneratePasswordResetTokenAsync(
                    It.IsAny<ApplicationUser>()),
                Times.Never);
        }

        // =========================================
        // ResetPasswordAsync
        // =========================================

        [Fact]
        public async Task ResetPasswordAsync_UserExists_ReturnsSuccess()
        {
            // Arrange
            var user = CreateUser();

            var request = new ResetPasswordDto
            {
                Email = "test@example.com",
                Token = "valid-token",
                NewPassword = "NewPassword123!"
            };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.ResetPasswordAsync(
                    user,
                    request.Token,
                    request.NewPassword))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _accountService.ResetPasswordAsync(request);

            // Assert
            Assert.True(result.Succeeded);

            _userManagerMock.Verify(
                x => x.ResetPasswordAsync(
                    user,
                    "valid-token",
                    "NewPassword123!"),
                Times.Once);
        }

        [Fact]
        public async Task ResetPasswordAsync_UserDoesNotExist_ReturnsFailedResult()
        {
            // Arrange
            var request = new ResetPasswordDto
            {
                Email = "unknown@example.com",
                Token = "token",
                NewPassword = "NewPassword123!"
            };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _accountService.ResetPasswordAsync(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains(
                result.Errors,
                e => e.Description == "User not found.");

            _userManagerMock.Verify(
                x => x.ResetPasswordAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task ResetPasswordAsync_InvalidToken_ReturnsFailedResult()
        {
            // Arrange
            var user = CreateUser();

            var request = new ResetPasswordDto
            {
                Email = "test@example.com",
                Token = "invalid-token",
                NewPassword = "NewPassword123!"
            };

            var identityResult = IdentityResult.Failed(
                new IdentityError
                {
                    Description = "Invalid token."
                });

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.ResetPasswordAsync(
                    user,
                    request.Token,
                    request.NewPassword))
                .ReturnsAsync(identityResult);

            // Act
            var result = await _accountService.ResetPasswordAsync(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains(
                result.Errors,
                e => e.Description == "Invalid token.");
        }
    }
}