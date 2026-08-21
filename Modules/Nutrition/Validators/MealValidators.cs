using backend.Modules.Nutrition.Contracts;
using backend.Modules.Nutrition.Domain.Constants;
using backend.Modules.Nutrition.Domain.Enums;
using FluentValidation;

namespace backend.Modules.Nutrition.Validators;

public class MealItemRequestValidator : AbstractValidator<MealItemRequest>
{
    public MealItemRequestValidator()
    {
        Include(new MealItemInputValidator<MealItemRequest>());
    }
}

public class AddMealItemRequestValidator : AbstractValidator<AddMealItemRequest>
{
    public AddMealItemRequestValidator()
    {
        RuleFor(request => request).Must(HasExactlyOneSource)
            .WithMessage("Provide either an item snapshot or a favourite food ID.");
        RuleFor(request => request.Item!).SetValidator(new MealItemRequestValidator()).When(request => request.Item is not null);
        RuleFor(request => request.FavouriteFoodId).NotEqual(Guid.Empty).When(request => request.FavouriteFoodId.HasValue);
    }

    private static bool HasExactlyOneSource(AddMealItemRequest request) =>
        (request.Item is null) != !request.FavouriteFoodId.HasValue;
}

public class CreateMealRequestValidator : AbstractValidator<CreateMealRequest>
{
    public CreateMealRequestValidator()
    {
        Include(new MealMetadataValidator<CreateMealRequest>());
        RuleFor(request => request.Items).NotEmpty().Must(items => items.Count <= NutritionLimits.MaximumItemsPerMeal);
        RuleForEach(request => request.Items).SetValidator(new MealItemRequestValidator());
    }
}

public class UpdateMealRequestValidator : AbstractValidator<UpdateMealRequest>
{
    public UpdateMealRequestValidator()
    {
        Include(new MealMetadataValidator<UpdateMealRequest>());
    }
}

public class ReorderMealItemsRequestValidator : AbstractValidator<ReorderMealItemsRequest>
{
    public ReorderMealItemsRequestValidator()
    {
        RuleFor(request => request.ItemIds).NotEmpty();
        RuleForEach(request => request.ItemIds).NotEqual(Guid.Empty);
        RuleFor(request => request.ItemIds).Must(itemIds => itemIds.Distinct().Count() == itemIds.Count)
            .WithMessage("Item IDs must be unique.");
    }
}

public class NutritionTargetRequestValidator : AbstractValidator<NutritionTargetRequest>
{
    public NutritionTargetRequestValidator()
    {
        RuleFor(request => request.CaloriesKcal).GreaterThan(0).LessThanOrEqualTo(NutritionLimits.MaximumDailyTarget)
            .When(request => request.CaloriesKcal.HasValue);
        RuleFor(request => request.ProteinGrams).GreaterThan(0).LessThanOrEqualTo(NutritionLimits.MaximumDailyTarget)
            .When(request => request.ProteinGrams.HasValue);
        RuleFor(request => request.CarbsGrams).GreaterThan(0).LessThanOrEqualTo(NutritionLimits.MaximumDailyTarget)
            .When(request => request.CarbsGrams.HasValue);
        RuleFor(request => request.FatGrams).GreaterThan(0).LessThanOrEqualTo(NutritionLimits.MaximumDailyTarget)
            .When(request => request.FatGrams.HasValue);
        RuleFor(request => request).Must(HasTarget).WithMessage("At least one nutrition target is required.");
    }

    private static bool HasTarget(NutritionTargetRequest request) => request.CaloriesKcal.HasValue || request.ProteinGrams.HasValue ||
        request.CarbsGrams.HasValue || request.FatGrams.HasValue;
}

public class FavouriteFoodRequestValidator : MealItemInputValidator<FavouriteFoodRequest>
{
}

public class FavouriteFoodFilterValidator : AbstractValidator<FavouriteFoodFilter>
{
    public FavouriteFoodFilterValidator()
    {
        RuleFor(filter => filter.Query).MaximumLength(NutritionLimits.MaximumFoodNameLength).When(filter => filter.Query is not null);
        RuleFor(filter => filter.Page).GreaterThan(0);
        RuleFor(filter => filter.PageSize).InclusiveBetween(1, 100);
    }
}

public class RecentFoodsFilterValidator : AbstractValidator<RecentFoodsFilter>
{
    public RecentFoodsFilterValidator() => RuleFor(filter => filter.Limit).InclusiveBetween(1, 50);
}

public class CommonFoodFilterValidator : AbstractValidator<CommonFoodFilter>
{
    public CommonFoodFilterValidator()
    {
        RuleFor(filter => filter.Query).MaximumLength(NutritionLimits.MaximumFoodNameLength).When(filter => filter.Query is not null);
        RuleFor(filter => filter.Category).MaximumLength(50).When(filter => filter.Category is not null);
    }
}

public class MealHistoryFilterValidator : AbstractValidator<MealHistoryFilter>
{
    public MealHistoryFilterValidator()
    {
        RuleFor(filter => filter.Page).GreaterThan(0);
        RuleFor(filter => filter.PageSize).InclusiveBetween(1, 100);
        RuleFor(filter => filter.Type).IsInEnum().When(filter => filter.Type.HasValue);
        RuleFor(filter => filter.To).GreaterThanOrEqualTo(filter => filter.From!.Value)
            .When(filter => filter.From.HasValue && filter.To.HasValue);
        RuleFor(filter => filter).Must(HasSupportedRange).When(filter => filter.From.HasValue && filter.To.HasValue)
            .WithMessage("Meal history date range must not exceed 366 days.");
    }

    private static bool HasSupportedRange(MealHistoryFilter filter) =>
        filter.To!.Value.DayNumber - filter.From!.Value.DayNumber <= 366;
}

public class NutritionSummaryFilterValidator : AbstractValidator<NutritionSummaryFilter>
{
    public NutritionSummaryFilterValidator()
    {
        RuleFor(filter => filter.To).GreaterThanOrEqualTo(filter => filter.From!.Value)
            .When(filter => filter.From.HasValue && filter.To.HasValue);
        RuleFor(filter => filter).Must(HasSupportedRange).When(filter => filter.From.HasValue && filter.To.HasValue)
            .WithMessage("Nutrition summary date range must not exceed 366 days.");
    }

    private static bool HasSupportedRange(NutritionSummaryFilter filter) =>
        filter.To!.Value.DayNumber - filter.From!.Value.DayNumber <= 366;
}

public class MealItemInputValidator<T> : AbstractValidator<T> where T : IMealItemInput
{
    public MealItemInputValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(NutritionLimits.MaximumFoodNameLength);
        RuleFor(request => request.Quantity).GreaterThan(0).LessThanOrEqualTo(NutritionLimits.MaximumQuantity);
        RuleFor(request => request.QuantityUnit).IsInEnum();
        RuleFor(request => request.CustomUnitName).NotEmpty().MaximumLength(NutritionLimits.MaximumCustomUnitNameLength)
            .When(request => request.QuantityUnit == QuantityUnit.CustomServing);
        RuleFor(request => request.CustomUnitName).Null().When(request => request.QuantityUnit != QuantityUnit.CustomServing);
        RuleFor(request => request.CaloriesKcal).InclusiveBetween(0, NutritionLimits.MaximumNutrientValue)
            .When(request => request.CaloriesKcal.HasValue);
        RuleFor(request => request.ProteinGrams).InclusiveBetween(0, NutritionLimits.MaximumNutrientValue)
            .When(request => request.ProteinGrams.HasValue);
        RuleFor(request => request.CarbsGrams).InclusiveBetween(0, NutritionLimits.MaximumNutrientValue)
            .When(request => request.CarbsGrams.HasValue);
        RuleFor(request => request.FatGrams).InclusiveBetween(0, NutritionLimits.MaximumNutrientValue)
            .When(request => request.FatGrams.HasValue);
        RuleFor(request => request).Must(HasNutritionValue).WithMessage("At least one nutrition value is required.");
    }

    private static bool HasNutritionValue(T request) => request.CaloriesKcal.HasValue || request.ProteinGrams.HasValue ||
        request.CarbsGrams.HasValue || request.FatGrams.HasValue;
}

public class MealMetadataValidator<T> : AbstractValidator<T> where T : IMealMetadataInput
{
    public MealMetadataValidator()
    {
        RuleFor(request => request.MealDate).NotEqual(DateOnly.MinValue);
        RuleFor(request => request.MealType).IsInEnum();
        RuleFor(request => request.Name).MaximumLength(NutritionLimits.MaximumMealNameLength).When(request => request.Name is not null);
        RuleFor(request => request.Notes).MaximumLength(NutritionLimits.MaximumMealNotesLength).When(request => request.Notes is not null);
    }
}
