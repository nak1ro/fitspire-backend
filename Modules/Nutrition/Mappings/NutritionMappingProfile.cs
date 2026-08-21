using AutoMapper;
using backend.Modules.Nutrition.Contracts;
using backend.Modules.Nutrition.Domain;

namespace backend.Modules.Nutrition.Mappings;

public class NutritionMappingProfile : Profile
{
    public NutritionMappingProfile()
    {
        CreateMap<MealItem, MealItemResponse>();
        CreateMap<NutritionTarget, NutritionTargetResponse>();
        CreateMap<FavouriteFood, FavouriteFoodResponse>();
        CreateMap<CommonFood, CommonFoodResponse>();
    }
}
