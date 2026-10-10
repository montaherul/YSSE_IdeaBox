using FluentValidation;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Application.Validators;

public sealed class InquiryCreateRequestValidator : AbstractValidator<InquiryCreateRequest>
{
    public InquiryCreateRequestValidator()
    {
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Message).NotEmpty().MinimumLength(10).MaximumLength(3000);
        RuleFor(x => x.IdempotencyKey).MaximumLength(100);
    }
}
