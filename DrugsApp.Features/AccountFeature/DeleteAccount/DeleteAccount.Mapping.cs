using System;

namespace DrugsApp.Features.AccountFeature.DeleteAccount;

internal static class DeleteAccountMapping
{
    public static DeleteAccountCommand MapToCommand(this DeleteAccountRequest request, Guid userId)
    {
        return new DeleteAccountCommand(userId, request);
    }
}
