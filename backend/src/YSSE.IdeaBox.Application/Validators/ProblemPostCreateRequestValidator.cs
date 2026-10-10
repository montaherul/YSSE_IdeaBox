using FluentValidation;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Application.Validators;

public sealed class ProblemPostCreateRequestValidator : AbstractValidator<ProblemPostCreateRequest>
{
    public ProblemPostCreateRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.WhoAffected).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.WhyItMatters).NotEmpty().MaximumLength(3000);
        RuleFor(x => x.CoarseLocation).MaximumLength(150);
        RuleFor(x => x.Urgency).MaximumLength(50);
        RuleFor(x => x.PhotoUrl).MaximumLength(500);
    }
}
