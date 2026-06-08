using FluentValidation;
using System;

namespace DrugsApp.Features.SupportFeature.RemoveFaqEntry;

/// <summary>
/// Validator for the FAQ delete request payload.
/// </summary>
public sealed class RemoveFaqEntryRequestValidator : AbstractValidator<RemoveFaqEntryRequest>
{
    public RemoveFaqEntryRequestValidator()
    {
        RuleFor(request => request.Id)
            .NotEqual(Guid.Empty)
            .WithMessage("Id must not be empty.");
    }
}
