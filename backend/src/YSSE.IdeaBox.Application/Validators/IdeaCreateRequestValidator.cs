using FluentValidation;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Application.Validators;

public sealed class IdeaCreateRequestValidator : AbstractValidator<IdeaCreateRequest>
{
    public IdeaCreateRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(120);
        RuleFor(x => x.ShortSummary).NotEmpty().MaximumLength(300);
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.EstimatedBudget).GreaterThanOrEqualTo(0).When(x => x.EstimatedBudget.HasValue);
        RuleFor(x => x.LanguageTag).MaximumLength(8);

        RuleFor(x => x.ProblemStatement)
            .NotEmpty().WithMessage("Problem statement is required.")
            .MaximumLength(5000)
            .When(x => !x.IsQuickIdea);

        RuleFor(x => x.ProposedSolution)
            .NotEmpty().WithMessage("Proposed solution is required.")
            .MaximumLength(5000)
            .When(x => !x.IsQuickIdea);

        RuleFor(x => x.TargetAudience).NotEmpty().MaximumLength(500).When(x => !x.IsQuickIdea);
        RuleFor(x => x.ExpectedImpact).NotEmpty().MaximumLength(500).When(x => !x.IsQuickIdea);
        RuleFor(x => x.SupportSought).MaximumLength(500);
        RuleFor(x => x.SkillsNeeded).MaximumLength(500);
        RuleFor(x => x.ExpectedTimeline).MaximumLength(200);
        RuleFor(x => x.SuccessMetrics).MaximumLength(1000);
        RuleFor(x => x.CollaborationPreferences).MaximumLength(500);
    }
}
