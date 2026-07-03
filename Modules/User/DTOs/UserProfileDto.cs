using backend.Modules.Media.Contracts;

namespace backend.Modules.User.DTOs;

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public Guid? ProfilePictureMediaId { get; set; }
    public MediaResponse? ProfilePicture { get; set; }
    public bool IsPrivate { get; set; }
}
