using backend.Modules.User.Domain.Enums;

namespace backend.Modules.User.DTOs;

public class UpdateProfileDto
{
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public bool? IsPrivate { get; set; }
    public FitnessSport? FavoriteSport { get; set; }
    public FitnessLevel? FitnessLevel { get; set; }
    public double? HeightCm { get; set; }
}
