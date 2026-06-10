using System;
using DrugsApp.Features.AccountFeature.Shared;

namespace DrugsApp.Features.AccountFeature.RequestLoginEmailChange;

internal static class RequestLoginEmailChangeMapping
{
    public static RequestLoginEmailChangeCommand MapToCommand(this RequestLoginEmailChangeRequest request, Guid userId)
    {
        return new RequestLoginEmailChangeCommand(userId, request);
    }
}
