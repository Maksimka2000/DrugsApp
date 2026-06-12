using System;

namespace DrugsApp.Features.AuthFeature.ResetPassword;

public static class ResetPasswordMapping
{
    public static ResetPasswordCommand MapToCommand(
        this ResetPasswordRequest request,
        Guid userId) =>
        new(userId, request);
}
