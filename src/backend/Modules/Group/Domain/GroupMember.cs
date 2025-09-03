using backend.Modules.User.Domain;

namespace backend.Modules.Group.Domain;

public class GroupMember
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public Guid UserId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public UserGroup UserGroup { get; set; } = null!;
    public AppUser User { get; set; } = null!;
}