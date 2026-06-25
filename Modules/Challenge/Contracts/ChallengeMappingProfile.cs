using AutoMapper;
using backend.Modules.Challenge.Domain;
using backend.Modules.User.Domain;

namespace backend.Modules.Challenge.Contracts;

public class ChallengeMappingProfile : Profile
{
    public ChallengeMappingProfile()
    {
        CreateMap<AppUser, ChallengeCreatorResponse>()
            .ForCtorParam(nameof(ChallengeCreatorResponse.UserId), options => options.MapFrom(user => user.Id))
            .ForCtorParam(nameof(ChallengeCreatorResponse.UserName), options => options.MapFrom(user => user.UserName ?? string.Empty))
            .ForCtorParam(nameof(ChallengeCreatorResponse.DisplayName), options => options.MapFrom(user => user.DisplayName))
            .ForCtorParam(nameof(ChallengeCreatorResponse.ProfilePictureUrl), options => options.MapFrom(user => user.ProfilePictureUrl));

        CreateMap<UserChallenge, ChallengeResponse>()
            .ForCtorParam(nameof(ChallengeResponse.ParticipantsCount), options => options.MapFrom(challenge => challenge.Participants.Count))
            .ForCtorParam(nameof(ChallengeResponse.IsJoined), options => options.MapFrom(_ => false));
    }
}
