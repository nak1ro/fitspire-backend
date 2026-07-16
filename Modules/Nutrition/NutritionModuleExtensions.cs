using backend.Modules.Nutrition.Contracts;
using backend.Modules.Nutrition.Services;
using backend.Modules.Nutrition.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Modules.Nutrition;

public static class NutritionModuleExtensions
{
    public static IServiceCollection AddNutritionModule(this IServiceCollection services)
    {
        services.AddScoped<IMealWriteService, MealWriteService>();
        services.AddScoped<INutritionTimeZoneService, NutritionTimeZoneService>();
        services.AddScoped<INutritionTargetService, NutritionTargetService>();
        services.AddScoped<IFavouriteFoodService, FavouriteFoodService>();
        services.AddScoped<IRecentFoodService, RecentFoodService>();
        services.AddScoped<INutritionReadService, NutritionReadService>();
        services.AddScoped<IValidator<MealItemRequest>, MealItemRequestValidator>();
        services.AddScoped<IValidator<AddMealItemRequest>, AddMealItemRequestValidator>();
        services.AddScoped<IValidator<CreateMealRequest>, CreateMealRequestValidator>();
        services.AddScoped<IValidator<UpdateMealRequest>, UpdateMealRequestValidator>();
        services.AddScoped<IValidator<ReorderMealItemsRequest>, ReorderMealItemsRequestValidator>();
        services.AddScoped<IValidator<NutritionTargetRequest>, NutritionTargetRequestValidator>();
        services.AddScoped<IValidator<FavouriteFoodRequest>, FavouriteFoodRequestValidator>();
        services.AddScoped<IValidator<FavouriteFoodFilter>, FavouriteFoodFilterValidator>();
        services.AddScoped<IValidator<RecentFoodsFilter>, RecentFoodsFilterValidator>();
        services.AddScoped<IValidator<MealHistoryFilter>, MealHistoryFilterValidator>();
        services.AddScoped<IValidator<NutritionSummaryFilter>, NutritionSummaryFilterValidator>();
        return services;
    }
}
