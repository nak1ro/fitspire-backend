using backend.Modules.User.Domain;

namespace backend.Modules.Group.Domain;

public class UserGroup
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsPrivate { get; set; } = false;
    public Guid? CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public AppUser? CreatedBy { get; set; }
    public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
}