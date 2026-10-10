using FluentValidation;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Application.Validators;

public sealed class MessageSendRequestValidator : AbstractValidator<MessageSendRequest>
{
    public MessageSendRequestValidator()
    {
        RuleFor(x => x.Body).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.IdempotencyKey).MaximumLength(100);
    }
}
