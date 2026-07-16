using backend.Modules.Nutrition.Domain.Enums;
using backend.Modules.Shared.Domain;

namespace backend.Modules.Nutrition.Domain.Constants;

public static class NutritionEntryRules
{
    public static void ValidateSnapshot(string? name, decimal quantity, QuantityUnit quantityUnit, string? customUnitName,
        decimal? caloriesKcal, decimal? proteinGrams, decimal? carbsGrams, decimal? fatGrams)
    {
        _ = NormalizeRequiredName(name, "Food name");
        if (!Enum.IsDefined(quantityUnit))
            throw new DomainException("Quantity unit is not supported.");
        if (quantity <= 0 || quantity > NutritionLimits.MaximumQuantity)
            throw new DomainException($"Quantity must be greater than zero and no more than {NutritionLimits.MaximumQuantity}.");

        _ = NormalizeCustomUnit(quantityUnit, customUnitName);
        ValidateNutrient(caloriesKcal, "Calories");
        ValidateNutrient(proteinGrams, "Protein");
        ValidateNutrient(carbsGrams, "Carbohydrates");
        ValidateNutrient(fatGrams, "Fat");
        if (!caloriesKcal.HasValue && !proteinGrams.HasValue && !carbsGrams.HasValue && !fatGrams.HasValue)
            throw new DomainException("At least one nutrition value is required.");
    }

    public static string NormalizeRequiredName(string? value, string label)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            throw new DomainException($"{label} is required.");
        if (normalized.Length > NutritionLimits.MaximumFoodNameLength)
            throw new DomainException($"{label} must be at most {NutritionLimits.MaximumFoodNameLength} characters.");
        return normalized;
    }

    public static string? NormalizeOptionalText(string? value, int maximumLength, string label)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        var normalized = value.Trim();
        if (normalized.Length > maximumLength)
            throw new DomainException($"{label} must be at most {maximumLength} characters.");
        return normalized;
    }

    public static string? NormalizeCustomUnit(QuantityUnit quantityUnit, string? customUnitName)
    {
        var normalized = NormalizeOptionalText(customUnitName, NutritionLimits.MaximumCustomUnitNameLength, "Custom unit name");
        if (quantityUnit == QuantityUnit.CustomServing && normalized is null)
            throw new DomainException("Custom unit name is required for a custom serving.");
        if (quantityUnit != QuantityUnit.CustomServing && normalized is not null)
            throw new DomainException("Custom unit name is only allowed for a custom serving.");
        return normalized;
    }

    public static void ValidateTarget(decimal? caloriesKcal, decimal? proteinGrams, decimal? carbsGrams, decimal? fatGrams)
    {
        ValidateTargetValue(caloriesKcal, "Calorie target");
        ValidateTargetValue(proteinGrams, "Protein target");
        ValidateTargetValue(carbsGrams, "Carbohydrate target");
        ValidateTargetValue(fatGrams, "Fat target");
        if (!caloriesKcal.HasValue && !proteinGrams.HasValue && !carbsGrams.HasValue && !fatGrams.HasValue)
            throw new DomainException("At least one nutrition target is required.");
    }

    public static string CreateDefinitionKey(string name, decimal quantity, QuantityUnit quantityUnit, string? customUnitName,
        decimal? caloriesKcal, decimal? proteinGrams, decimal? carbsGrams, decimal? fatGrams) =>
        string.Join('|', name.Trim().ToUpperInvariant(), quantity, quantityUnit, customUnitName?.Trim().ToUpperInvariant() ?? string.Empty,
            caloriesKcal?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            proteinGrams?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            carbsGrams?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            fatGrams?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty);

    private static void ValidateNutrient(decimal? value, string label)
    {
        if (value is < 0 or > NutritionLimits.MaximumNutrientValue)
            throw new DomainException($"{label} must be between zero and {NutritionLimits.MaximumNutrientValue}.");
    }

    private static void ValidateTargetValue(decimal? value, string label)
    {
        if (value is <= 0 or > NutritionLimits.MaximumDailyTarget)
            throw new DomainException($"{label} must be greater than zero and no more than {NutritionLimits.MaximumDailyTarget}.");
    }
}
