using AutoMapper;
using backend.Modules.Moderation.Domain;

namespace backend.Modules.Moderation.Contracts;

public sealed class ModerationReportMappingProfile : Profile
{
    public ModerationReportMappingProfile()
    {
        CreateMap<ModerationReport, ModerationReportSubmissionResponse>()
            .ConstructUsing(report => new ModerationReportSubmissionResponse(
                report.Id,
                report.Status,
                report.CreatedAt,
                false));

        CreateMap<ModerationAction, AdminModerationActionResponse>()
            .ConstructUsing(action => new AdminModerationActionResponse(
                action.Id,
                action.ActionType,
                action.OccurredAtUtc,
                action.Note,
                action.SuspensionEndsAtUtc,
                new AdminModerationUserResponse(
                    action.ModeratorUser.Id,
                    action.ModeratorUser.UserName ?? string.Empty,
                    action.ModeratorUser.DisplayName)));
    }
}
