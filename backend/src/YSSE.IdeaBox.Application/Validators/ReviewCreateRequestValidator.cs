using FluentValidation;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Application.Validators;

public sealed class ReviewCreateRequestValidator : AbstractValidator<ReviewCreateRequest>
{
    public ReviewCreateRequestValidator()
    {
        RuleFor(x => x.Body).NotEmpty().MinimumLength(10).MaximumLength(3000);
        RuleFor(x => x.Rating).InclusiveBetween(1, 5).When(x => x.Rating.HasValue);
    }
}
