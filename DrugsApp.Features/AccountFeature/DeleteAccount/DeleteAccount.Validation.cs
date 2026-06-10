using FluentValidation;

namespace DrugsApp.Features.AccountFeature.DeleteAccount;

public sealed class DeleteAccountRequestValidator : AbstractValidator<DeleteAccountRequest>
{
    private const string RequiredPhrase = "DELETE MY ACCOUNT";

    public DeleteAccountRequestValidator()
    {
        RuleFor(r => r.CurrentPassword)
            .NotEmpty();

        RuleFor(r => r.ConfirmationPhrase)
            .Equal(RequiredPhrase)
            .WithMessage($"Confirmation phrase must be '{RequiredPhrase}'.");
    }
}
