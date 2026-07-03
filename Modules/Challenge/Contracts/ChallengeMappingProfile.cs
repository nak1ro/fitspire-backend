using AutoMapper;
using backend.Modules.Challenge.Domain;

namespace backend.Modules.Challenge.Contracts;

public class ChallengeMappingProfile : Profile
{
    public ChallengeMappingProfile()
    {
        CreateMap<UserChallenge, ChallengeResponse>()
            .ForCtorParam(nameof(ChallengeResponse.ParticipantsCount), options => options.MapFrom(challenge => challenge.Participants.Count))
            .ForCtorParam(nameof(ChallengeResponse.IsJoined), options => options.MapFrom(_ => false));
    }
}
