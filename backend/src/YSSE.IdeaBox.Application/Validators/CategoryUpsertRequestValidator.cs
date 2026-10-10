using FluentValidation;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Application.Validators;

public sealed class CategoryUpsertRequestValidator : AbstractValidator<CategoryUpsertRequest>
{
    public CategoryUpsertRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Slug).MaximumLength(120);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
