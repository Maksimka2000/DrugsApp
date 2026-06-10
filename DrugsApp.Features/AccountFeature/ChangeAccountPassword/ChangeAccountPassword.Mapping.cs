using System;

namespace DrugsApp.Features.AccountFeature.ChangeAccountPassword;

internal static class ChangeAccountPasswordMapping
{
    public static ChangeAccountPasswordCommand MapToCommand(this ChangeAccountPasswordRequest request, Guid userId)
    {
        return new ChangeAccountPasswordCommand(userId, request);
    }
}
