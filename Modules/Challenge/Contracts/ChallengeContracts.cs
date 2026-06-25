using FluentValidation;

namespace backend.Modules.Challenge.Contracts;

public record CreateChallengeRequest(string Title, string? Description, string MetricCode, string? WorkoutType, string Mode, double? TargetValue,
    string Visibility, DateTime StartDate, DateTime EndDate, string JoinClosing, int ParticipantLimit = 100);
public record InviteChallengeUserRequest(Guid UserId);
public record ChallengeResponse(Guid Id, string Title, string? Description, string MetricCode, string? WorkoutType, string Mode, double? TargetValue,
    string Visibility, DateTime StartDate, DateTime EndDate, string JoinClosing, int ParticipantLimit, string Status, int ParticipantsCount, bool IsJoined);
public record ChallengeLeaderboardEntry(Guid UserId, string DisplayName, double Score, int Rank);

public class CreateChallengeRequestValidator : AbstractValidator<CreateChallengeRequest>
{
    public CreateChallengeRequestValidator()
    {
        RuleFor(item => item.Title).NotEmpty().MaximumLength(120); RuleFor(item => item.Description).MaximumLength(1000);
        RuleFor(item => item.MetricCode).NotEmpty().MaximumLength(80); RuleFor(item => item.Mode).Must(item => item is "Target" or "Leaderboard");
        RuleFor(item => item.Visibility).Must(item => item is "Public" or "FollowersOnly" or "InviteOnly"); RuleFor(item => item.JoinClosing).Must(item => item is "AtStart" or "AtEnd");
        RuleFor(item => item.EndDate).GreaterThan(item => item.StartDate); RuleFor(item => item).Must(item => item.EndDate <= item.StartDate.AddYears(1));
        RuleFor(item => item.ParticipantLimit).InclusiveBetween(2, 100); RuleFor(item => item.TargetValue).GreaterThan(0).When(item => item.Mode == "Target");
    }
}
public class InviteChallengeUserRequestValidator : AbstractValidator<InviteChallengeUserRequest> { public InviteChallengeUserRequestValidator() { RuleFor(item => item.UserId).NotEmpty(); } }
