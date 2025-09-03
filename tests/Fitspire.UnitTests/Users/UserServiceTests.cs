using System.Security.Claims;
using AutoMapper;
using backend.Modules.Shared.Service;
using backend.Modules.User.Domain;
using backend.Modules.User.DTOs;
using backend.Modules.User.Repositories;
using backend.Modules.User.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Fitspire.UnitTests.Users
{
    public class UserServiceTests
    {
        private readonly Mock<UserManager<AppUser>> _userManagerMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
        private readonly Mock<IBlobService> _blobServiceMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IUserRepository> _userRepositoryMock = new();

        public UserServiceTests()
        {
            var store = new Mock<IUserStore<AppUser>>();
            _userManagerMock = new Mock<UserManager<AppUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }

        private static ClaimsPrincipal CreatePrincipal(Guid userId)
        {
            var identity = new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            ], "TestAuth");
            return new ClaimsPrincipal(identity);
        }

        private UserService CreateSut() => new(
            _userManagerMock.Object,
            _httpContextAccessorMock.Object,
            _blobServiceMock.Object,
            _mapperMock.Object,
            _userRepositoryMock.Object);

        private static DefaultHttpContext HttpContextWithUser(Guid userId)
        {
            return new DefaultHttpContext { User = CreatePrincipal(userId) };
        }

        private static IFormFile CreateFormFile(string fileName, string contentType, byte[] bytes)
        {
            var stream = new MemoryStream(bytes);
            stream.Position = 0;
            return new FormFile(stream, 0, bytes.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }

        [Fact]
        public async Task GetProfileAsync_WhenAuthenticated_ReturnsMappedProfile()
        {
            var userId = Guid.NewGuid();
            var user = new AppUser { Id = userId, UserName = "john", DisplayName = "John" };
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(HttpContextWithUser(userId));
            _userRepositoryMock.Setup(r => r.GetByIdWithPrefsAsync(userId)).ReturnsAsync(user);

            var expected = new UserProfileDto { Id = userId, UserName = "john", DisplayName = "John" };
            _mapperMock.Setup(m => m.Map<UserProfileDto>(user)).Returns(expected);

            var sut = CreateSut();
            
            var dto = await sut.GetProfileAsync();

            dto.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task GetProfileAsync_WhenNoUserClaim_ThrowsUnauthorized()
        {
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
            var sut = CreateSut();

            var act = () => sut.GetProfileAsync();

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("User not found.");
        }

        [Fact]
        public async Task GetProfileAsync_WhenRepoReturnsNull_ThrowsUnauthorized()
        {
            var userId = Guid.NewGuid();
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(HttpContextWithUser(userId));
            _userRepositoryMock.Setup(r => r.GetByIdWithPrefsAsync(userId)).ReturnsAsync((AppUser?)null);
            var sut = CreateSut();

            var act = () => sut.GetProfileAsync();

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("User not found.");
        }

        [Fact]
        public async Task UpdateProfileAsync_WhenValid_UpdatesUserAndReturnsMapped()
        {
            var userId = Guid.NewGuid();
            var user = new AppUser { Id = userId, DisplayName = "Old", Bio = "Old bio" };
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(HttpContextWithUser(userId));
            _userRepositoryMock.Setup(r => r.GetByIdWithPrefsAsync(userId)).ReturnsAsync(user);

            AppUser? updatedUserPassed = null;
            _userManagerMock.Setup(um => um.UpdateAsync(It.IsAny<AppUser>()))
                .Callback<AppUser>(u => updatedUserPassed = u)
                .ReturnsAsync(IdentityResult.Success);

            var expected = new UserProfileDto { Id = userId, DisplayName = "New Name", Bio = "New bio" };
            _mapperMock.Setup(m => m.Map<UserProfileDto>(It.IsAny<AppUser>())).Returns(expected);

            var sut = CreateSut();
            var dto = new UpdateProfileDto { DisplayName = "New Name", Bio = "New bio" };

            var result = await sut.UpdateProfileAsync(dto);

            result.Should().BeEquivalentTo(expected);
            updatedUserPassed.Should().NotBeNull();
            updatedUserPassed!.DisplayName.Should().Be("New Name");
            updatedUserPassed.Bio.Should().Be("New bio");
        }

        [Fact]
        public async Task UpdateProfileAsync_WhenUpdateFails_ThrowsInvalidOperationException()
        {
            var userId = Guid.NewGuid();
            var user = new AppUser { Id = userId };
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(HttpContextWithUser(userId));
            _userRepositoryMock.Setup(r => r.GetByIdWithPrefsAsync(userId)).ReturnsAsync(user);

            var failure = IdentityResult.Failed(new IdentityError { Description = "boom" }, new IdentityError { Description = "pow" });
            _userManagerMock.Setup(um => um.UpdateAsync(It.IsAny<AppUser>())).ReturnsAsync(failure);

            var sut = CreateSut();

            var act = () => sut.UpdateProfileAsync(new UpdateProfileDto { DisplayName = "x" });

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("boom; pow");
        }

        [Fact]
        public async Task UpdateProfilePictureAsync_WhenUploadFails_WrapsException()
        {
            var userId = Guid.NewGuid();
            var user = new AppUser { Id = userId };
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(HttpContextWithUser(userId));
            _userRepositoryMock.Setup(r => r.GetByIdWithPrefsAsync(userId)).ReturnsAsync(user);

            _blobServiceMock.Setup(b => b.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("storage down"));

            var sut = CreateSut();
            var formFile = CreateFormFile("avatar.png", "image/png", [1, 2, 3, 4]);

            var act = () => sut.UpdateProfilePictureAsync(formFile);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("File upload failed, please try again.");
        }

        [Fact]
        public async Task UpdateProfilePictureAsync_WhenTooLarge_ThrowsArgumentException()
        {
            var userId = Guid.NewGuid();
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(HttpContextWithUser(userId));
            _userRepositoryMock.Setup(r => r.GetByIdWithPrefsAsync(userId)).ReturnsAsync(new AppUser { Id = userId });

            var fileMock = new Mock<IFormFile>();
            fileMock.SetupGet(f => f.Length).Returns(long.MaxValue); 
            fileMock.SetupGet(f => f.ContentType).Returns("image/png");
            fileMock.SetupGet(f => f.FileName).Returns("big.png");

            var sut = CreateSut();

            var act = () => sut.UpdateProfilePictureAsync(fileMock.Object);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("File is too large. Maximum size is 5 MB.");
        }

        [Fact]
        public async Task UpdateProfilePictureAsync_WhenUnsupportedExtension_ThrowsArgumentException()
        {
            var userId = Guid.NewGuid();
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(HttpContextWithUser(userId));
            _userRepositoryMock.Setup(r => r.GetByIdWithPrefsAsync(userId)).ReturnsAsync(new AppUser { Id = userId });

            var formFile = CreateFormFile("avatar.gif", "image/png", new byte[] { 1 });
            var sut = CreateSut();

            var act = () => sut.UpdateProfilePictureAsync(formFile);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Unsupported file extension. Only .jpg, .jpeg, .png, .webp are allowed.");
        }

        [Fact]
        public async Task GetPreferencesAsync_EnsuresPrefsAndMaps()
        {
            var userId = Guid.NewGuid();
            var user = new AppUser { Id = userId };
            var prefs = new AppUserPreference
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PreferredLanguage = "pl",
                IsDarkModeEnabled = true,
                ReceiveEmailNotifications = false,
                UnitSystem = "metric"
            };

            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(HttpContextWithUser(userId));
            _userRepositoryMock.Setup(r => r.GetByIdWithPrefsAsync(userId)).ReturnsAsync(user);
            _userRepositoryMock.Setup(r => r.EnsurePreferencesAsync(userId)).ReturnsAsync(prefs);

            var expected = new UserPreferencesDto
            {
                PreferredLanguage = "pl",
                IsDarkModeEnabled = true,
                ReceiveEmailNotifications = false,
                UnitSystem = "metric"
            };
            _mapperMock.Setup(m => m.Map<UserPreferencesDto>(prefs)).Returns(expected);

            var sut = CreateSut();

            var dto = await sut.GetPreferencesAsync();

            dto.Should().BeEquivalentTo(expected);
            user.AppUserPreference.Should().BeSameAs(prefs); 
        }

        [Fact]
        public async Task GetUserByUsernameAsync_WhenNotFound_ThrowsKeyNotFoundException()
        {
            _userRepositoryMock.Setup(r => r.GetByUsernameAsync("ghost")).ReturnsAsync((AppUser?)null);
            var sut = CreateSut();

            var act = () => sut.GetUserByUsernameAsync("ghost");

            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("User with username 'ghost' not found.");
        }

        [Fact]
        public async Task GetUserByUsernameAsync_WhenFound_MapsAndReturns()
        {
            var user = new AppUser { Id = Guid.NewGuid(), UserName = "john" };
            _userRepositoryMock.Setup(r => r.GetByUsernameAsync("john")).ReturnsAsync(user);
            var expected = new UserProfileDto { Id = user.Id, UserName = "john" };
            _mapperMock.Setup(m => m.Map<UserProfileDto>(user)).Returns(expected);
            var sut = CreateSut();

            var dto = await sut.GetUserByUsernameAsync("john");
            
            dto.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task UpdatePreferencesAsync_AppliesChanges_UpdatesNavigation_AndMaps()
        {
            var userId = Guid.NewGuid();
            var user = new AppUser { Id = userId };
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(HttpContextWithUser(userId));
            _userRepositoryMock.Setup(r => r.GetByIdWithPrefsAsync(userId)).ReturnsAsync(user);

            AppUserPreference? actualPrefs = null;
            _userRepositoryMock
                .Setup(r => r.UpsertPreferencesAsync(userId, It.IsAny<Action<AppUserPreference>>()))
                .ReturnsAsync((Guid _, Action<AppUserPreference> apply) =>
                {
                    var p = new AppUserPreference
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        PreferredLanguage = "en",
                        IsDarkModeEnabled = false,
                        ReceiveEmailNotifications = true,
                        UnitSystem = "metric"
                    };
                    apply(p);
                    actualPrefs = p;
                    return p;
                });

            var expected = new UserPreferencesDto
            {
                PreferredLanguage = "pl",
                IsDarkModeEnabled = true,
                ReceiveEmailNotifications = false,
                UnitSystem = "imperial"
            };
            _mapperMock.Setup(m => m.Map<UserPreferencesDto>(It.IsAny<AppUserPreference>())).Returns(expected);

            var sut = CreateSut();
            var dto = new UpdateUserPreferencesDto
            {
                PreferredLanguage = "pl",
                IsDarkModeEnabled = true,
                ReceiveEmailNotifications = false,
                UnitSystem = "imperial"
            };

            var result = await sut.UpdatePreferencesAsync(dto);

            result.Should().BeEquivalentTo(expected);
            actualPrefs.Should().NotBeNull();
            actualPrefs!.PreferredLanguage.Should().Be("pl");
            actualPrefs.IsDarkModeEnabled.Should().BeTrue();
            actualPrefs.ReceiveEmailNotifications.Should().BeFalse();
            actualPrefs.UnitSystem.Should().Be("imperial");
            user.AppUserPreference.Should().BeSameAs(actualPrefs);
        }
    }
}
