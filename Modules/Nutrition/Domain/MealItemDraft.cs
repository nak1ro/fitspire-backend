using backend.Modules.Nutrition.Domain.Enums;

namespace backend.Modules.Nutrition.Domain;

public sealed record MealItemDraft(
    string Name,
    decimal Quantity,
    QuantityUnit QuantityUnit,
    string? CustomUnitName,
    decimal? CaloriesKcal,
    decimal? ProteinGrams,
    decimal? CarbsGrams,
    decimal? FatGrams,
    Guid? FavouriteFoodId = null);
