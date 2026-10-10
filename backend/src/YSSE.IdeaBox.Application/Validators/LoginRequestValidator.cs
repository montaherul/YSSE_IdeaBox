using FluentValidation;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Application.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.EmailOrPhone).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}
