using FluentValidation;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Application.Validators;

public sealed class ListingCreateRequestValidator : AbstractValidator<ListingCreateRequest>
{
    public ListingCreateRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(3000);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Condition).NotEmpty().MaximumLength(60);
        RuleFor(x => x.ListingType).IsInEnum();
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).When(x => x.Price.HasValue);
        RuleFor(x => x.CoarseLocation).MaximumLength(150);
        RuleFor(x => x.PhotoUrl).MaximumLength(500);
    }
}
