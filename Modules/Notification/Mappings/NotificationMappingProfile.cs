using AutoMapper;
using backend.Modules.Notification.Domain;
using backend.Modules.Notification.DTOs;

namespace backend.Modules.Notification.Mappings;

public class NotificationMappingProfile : Profile
{
    public NotificationMappingProfile()
    {
        CreateMap<AppNotification, NotificationResponse>()
            .ForCtorParam(nameof(NotificationResponse.Type), opt => opt.MapFrom(s => s.Type.ToString()));
    }
}
