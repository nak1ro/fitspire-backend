using backend.Modules.Nutrition.Domain.Enums;

namespace backend.Modules.Nutrition.Contracts;

public interface IMealItemInput
{
    string Name { get; }
    decimal Quantity { get; }
    QuantityUnit QuantityUnit { get; }
    string? CustomUnitName { get; }
    decimal? CaloriesKcal { get; }
    decimal? ProteinGrams { get; }
    decimal? CarbsGrams { get; }
    decimal? FatGrams { get; }
}

public interface IMealMetadataInput
{
    DateOnly MealDate { get; }
    MealType MealType { get; }
    string? Name { get; }
    string? Notes { get; }
}

public record MealItemRequest(string Name, decimal Quantity, QuantityUnit QuantityUnit, string? CustomUnitName,
    decimal? CaloriesKcal, decimal? ProteinGrams, decimal? CarbsGrams, decimal? FatGrams) : IMealItemInput;

public record AddMealItemRequest(MealItemRequest? Item = null, Guid? FavouriteFoodId = null);

public record CreateMealRequest(DateOnly MealDate, TimeOnly? ConsumedAtLocalTime, MealType MealType, string? Name,
    string? Notes, IReadOnlyList<MealItemRequest> Items) : IMealMetadataInput;

public record UpdateMealRequest(DateOnly MealDate, TimeOnly? ConsumedAtLocalTime, MealType MealType, string? Name,
    string? Notes) : IMealMetadataInput;

public record ReorderMealItemsRequest(IReadOnlyList<Guid> ItemIds);

public record MealItemResponse(Guid Id, string Name, decimal Quantity, QuantityUnit QuantityUnit, string? CustomUnitName,
    decimal? CaloriesKcal, decimal? ProteinGrams, decimal? CarbsGrams, decimal? FatGrams, int OrderIndex,
    Guid? FavouriteFoodId, DateTime CreatedAt, DateTime? UpdatedAt);

public record MealResponse(Guid Id, DateOnly MealDate, TimeOnly? ConsumedAtLocalTime, MealType MealType, string? Name,
    string? Notes, IReadOnlyList<MealItemResponse> Items, decimal CaloriesKcal, decimal ProteinGrams,
    decimal CarbsGrams, decimal FatGrams, DateTime CreatedAt, DateTime? UpdatedAt);

public record NutritionTargetRequest(decimal? CaloriesKcal, decimal? ProteinGrams, decimal? CarbsGrams, decimal? FatGrams);

public record NutritionTargetResponse(Guid Id, decimal? CaloriesKcal, decimal? ProteinGrams, decimal? CarbsGrams,
    decimal? FatGrams, DateTime CreatedAt, DateTime? UpdatedAt);

public record FavouriteFoodRequest(string Name, decimal Quantity, QuantityUnit QuantityUnit, string? CustomUnitName,
    decimal? CaloriesKcal, decimal? ProteinGrams, decimal? CarbsGrams, decimal? FatGrams) : IMealItemInput;

public record FavouriteFoodResponse(Guid Id, string Name, decimal Quantity, QuantityUnit QuantityUnit, string? CustomUnitName,
    decimal? CaloriesKcal, decimal? ProteinGrams, decimal? CarbsGrams, decimal? FatGrams, DateTime CreatedAt,
    DateTime? UpdatedAt);

public record FavouriteFoodFilter(string? Query = null, int Page = 1, int PageSize = 20);
public record FavouriteFoodPageResponse(IReadOnlyList<FavouriteFoodResponse> Items, int Page, int PageSize, int TotalCount);

public record CommonFoodFilter(string? Query = null, string? Category = null);
public record CommonFoodResponse(Guid Id, string Name, string Category, decimal Quantity, QuantityUnit QuantityUnit,
    string? CustomUnitName, decimal? CaloriesKcal, decimal? ProteinGrams, decimal? CarbsGrams, decimal? FatGrams);
public record RecentFoodsFilter(int Limit = 10);
public record RecentFoodResponse(string Name, decimal Quantity, QuantityUnit QuantityUnit, string? CustomUnitName,
    decimal? CaloriesKcal, decimal? ProteinGrams, decimal? CarbsGrams, decimal? FatGrams, DateOnly LastUsedDate,
    TimeOnly? LastUsedLocalTime);

public record MealHistoryFilter(DateOnly? From = null, DateOnly? To = null, MealType? Type = null, int Page = 1,
    int PageSize = 20);

public record MealPageResponse(IReadOnlyList<MealResponse> Items, int Page, int PageSize, int TotalCount);

public record NutritionTotalsResponse(decimal CaloriesKcal, decimal ProteinGrams, decimal CarbsGrams, decimal FatGrams);

public record NutritionTargetProgressResponse(decimal? Target, decimal? Percentage);

public record DailyNutritionSummaryResponse(DateOnly Date, IReadOnlyList<MealResponse> Meals,
    NutritionTotalsResponse Totals, NutritionTargetResponse? Target,
    NutritionTargetProgressResponse CaloriesKcalProgress, NutritionTargetProgressResponse ProteinGramsProgress,
    NutritionTargetProgressResponse CarbsGramsProgress, NutritionTargetProgressResponse FatGramsProgress);

public record NutritionSummaryFilter(DateOnly? From = null, DateOnly? To = null);

public record NutritionDailyTotalPoint(DateOnly Date, NutritionTotalsResponse Totals);

public record NutritionRangeSummaryResponse(DateOnly From, DateOnly To, int CalendarDayCount, int LoggedDayCount,
    NutritionTotalsResponse Totals, NutritionTotalsResponse? AveragePerLoggedDay, NutritionTargetResponse? Target,
    NutritionTargetProgressResponse CaloriesKcalAverageProgress,
    NutritionTargetProgressResponse ProteinGramsAverageProgress,
    NutritionTargetProgressResponse CarbsGramsAverageProgress,
    NutritionTargetProgressResponse FatGramsAverageProgress, IReadOnlyList<NutritionDailyTotalPoint> DailyTotals);
