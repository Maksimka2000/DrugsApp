using System;

namespace DrugsApp.Features.AuthFeature.ConfirmEmail;

public static class ConfirmEmailMapping
{
    public static ConfirmEmailCommand MapToCommand(
        this ConfirmEmailRequest request,
        Guid userId) => new(userId, request);
}
