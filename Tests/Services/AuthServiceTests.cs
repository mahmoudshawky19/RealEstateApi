
using Application.DTOs.Auth;
using Infrastructure.Identity;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly AuthService _authService;
        private readonly IConfiguration _configuration;

        public AuthServiceTests()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();

            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                store.Object,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!);

            // JWT settings used in tests
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:SecretKey"] = "ThisIsATestSecretKeyWithMoreThan32Characters!",
                    ["Jwt:Issuer"] = "TestIssuer",
                    ["Jwt:Audience"] = "TestAudience"
                })
                .Build();

            _authService = new AuthService(
                _userManagerMock.Object,
                _configuration);
        }

        private ApplicationUser CreateUser()
        {
            return new ApplicationUser
            {
                Id = "user-1",
                UserName = "test@example.com",
                Email = "test@example.com",
                FullName = "Test User"
            };
        }

        private RegisterRequestDto CreateRegisterRequest()
        {
            return new RegisterRequestDto
            {
                Email = "test@example.com",
                Password = "Password123!",
                FullName = "Test User",
                CompanyName = "Test Company"
            };
        }

        private LoginRequestDto CreateLoginRequest()
        {
            return new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "Password123!"
            };
        }

        // =========================================
        // RegisterAsync
        // =========================================

        [Fact]
        public async Task RegisterAsync_ValidRequest_ReturnsAuthResponseWithToken()
        {
            // Arrange
            var request = CreateRegisterRequest();

            _userManagerMock
                .Setup(x => x.CreateAsync(
                    It.IsAny<ApplicationUser>(),
                    request.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(x => x.AddToRoleAsync(
                    It.IsAny<ApplicationUser>(),
                    RoleNames.User))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(x => x.GetRolesAsync(
                    It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string> { RoleNames.User });

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.Email, result.Email);
            Assert.Equal(request.FullName, result.FullName);
            Assert.False(string.IsNullOrWhiteSpace(result.Token));

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(result.Token);

            Assert.Equal("TestIssuer", jwt.Issuer);
            Assert.Equal("TestAudience", jwt.Audiences.First());

            Assert.Contains(
                jwt.Claims,
                c => c.Type == ClaimTypes.Name &&
                     c.Value == request.FullName);

            Assert.Contains(
                jwt.Claims,
                c => c.Type == ClaimTypes.Email &&
                     c.Value == request.Email);

            Assert.Contains(
                jwt.Claims,
                c => c.Type == ClaimTypes.Role &&
                     c.Value == RoleNames.User);

            _userManagerMock.Verify(
                x => x.CreateAsync(
                    It.IsAny<ApplicationUser>(),
                    request.Password),
                Times.Once);

            _userManagerMock.Verify(
                x => x.AddToRoleAsync(
                    It.IsAny<ApplicationUser>(),
                    RoleNames.User),
                Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_CreateUserFails_ThrowsInvalidOperationException()
        {
            // Arrange
            var request = CreateRegisterRequest();

            _userManagerMock
                .Setup(x => x.CreateAsync(
                    It.IsAny<ApplicationUser>(),
                    request.Password))
                .ReturnsAsync(
                    IdentityResult.Failed(
                        new IdentityError
                        {
                            Description = "Password is too weak."
                        }));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _authService.RegisterAsync(request));

            Assert.Equal("Failed to create user.", exception.Message);

            _userManagerMock.Verify(
                x => x.AddToRoleAsync(
                    It.IsAny<ApplicationUser>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_AddToRoleFails_ThrowsInvalidOperationException()
        {
            // Arrange
            var request = CreateRegisterRequest();

            _userManagerMock
                .Setup(x => x.CreateAsync(
                    It.IsAny<ApplicationUser>(),
                    request.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(x => x.AddToRoleAsync(
                    It.IsAny<ApplicationUser>(),
                    RoleNames.User))
                .ReturnsAsync(
                    IdentityResult.Failed(
                        new IdentityError
                        {
                            Description = "Role does not exist."
                        }));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _authService.RegisterAsync(request));

            Assert.Contains("Role does not exist.", exception.Message);

            _userManagerMock.Verify(
                x => x.GetRolesAsync(It.IsAny<ApplicationUser>()),
                Times.Never);
        }

        // =========================================
        // LoginAsync
        // =========================================

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsAuthResponseWithToken()
        {
            // Arrange
            var request = CreateLoginRequest();
            var user = CreateUser();

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.CheckPasswordAsync(user, request.Password))
                .ReturnsAsync(true);

            _userManagerMock
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { RoleNames.User });

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.UserId);
            Assert.Equal(user.Email, result.Email);
            Assert.Equal(user.FullName, result.FullName);
            Assert.False(string.IsNullOrWhiteSpace(result.Token));

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(result.Token);

            Assert.Equal("TestIssuer", jwt.Issuer);
            Assert.Equal("TestAudience", jwt.Audiences.First());

            Assert.Contains(
                jwt.Claims,
                c => c.Type == ClaimTypes.NameIdentifier &&
                     c.Value == user.Id);

            Assert.Contains(
                jwt.Claims,
                c => c.Type == ClaimTypes.Role &&
                     c.Value == RoleNames.User);

            _userManagerMock.Verify(
                x => x.CheckPasswordAsync(user, request.Password),
                Times.Once);
        }

        [Fact]
        public async Task LoginAsync_UserDoesNotExist_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var request = CreateLoginRequest();

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync((ApplicationUser?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.LoginAsync(request));

            Assert.Equal(
                "Invalid email or password.",
                exception.Message);

            _userManagerMock.Verify(
                x => x.CheckPasswordAsync(
                    It.IsAny<ApplicationUser>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var request = CreateLoginRequest();
            var user = CreateUser();

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.CheckPasswordAsync(user, request.Password))
                .ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.LoginAsync(request));

            Assert.Equal(
                "Invalid email or password.",
                exception.Message);

            _userManagerMock.Verify(
                x => x.GetRolesAsync(It.IsAny<ApplicationUser>()),
                Times.Never);
        }
    }
}
