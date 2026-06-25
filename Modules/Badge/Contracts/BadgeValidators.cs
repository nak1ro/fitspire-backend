using backend.Modules.Badge.Domain.Constants;
using FluentValidation;

namespace backend.Modules.Badge.Contracts;

public class BadgeCatalogueFilterValidator : AbstractValidator<BadgeCatalogueFilter>
{
    public BadgeCatalogueFilterValidator()
    {
        ConfigurePagination();
        RuleFor(filter => filter.Category)
            .Must(category => string.IsNullOrWhiteSpace(category) || BadgeCategories.IsKnown(category))
            .WithMessage("Badge category is not supported.");
    }

    private void ConfigurePagination()
    {
        RuleFor(filter => filter.Page).GreaterThan(0);
        RuleFor(filter => filter.PageSize).InclusiveBetween(1, 100);
    }
}

public class BadgeCollectionFilterValidator : AbstractValidator<BadgeCollectionFilter>
{
    public BadgeCollectionFilterValidator()
    {
        RuleFor(filter => filter.Page).GreaterThan(0);
        RuleFor(filter => filter.PageSize).InclusiveBetween(1, 100);
        RuleFor(filter => filter.Category)
            .Must(category => string.IsNullOrWhiteSpace(category) || BadgeCategories.IsKnown(category))
            .WithMessage("Badge category is not supported.");
    }
}

public class PublicBadgeFilterValidator : AbstractValidator<PublicBadgeFilter>
{
    public PublicBadgeFilterValidator()
    {
        RuleFor(filter => filter.Page).GreaterThan(0);
        RuleFor(filter => filter.PageSize).InclusiveBetween(1, 100);
        RuleFor(filter => filter.Category)
            .Must(category => string.IsNullOrWhiteSpace(category) || BadgeCategories.IsKnown(category))
            .WithMessage("Badge category is not supported.");
    }
}

public class SetFeaturedBadgesRequestValidator : AbstractValidator<SetFeaturedBadgesRequest>
{
    public SetFeaturedBadgesRequestValidator()
    {
        RuleFor(request => request.BadgeIds).NotNull().Must(ids => ids.Count <= 5)
            .WithMessage("Select up to five earned badges.");
        RuleFor(request => request.BadgeIds).Must(ids => ids.Distinct().Count() == ids.Count)
            .When(request => request.BadgeIds is not null)
            .WithMessage("Featured badges must be unique.");
        RuleForEach(request => request.BadgeIds).NotEmpty();
    }
}
