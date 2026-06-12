namespace DrugsApp.Features.AuthFeature.ForgotPassword;

public static class ForgotPasswordMapping
{
    public static ForgotPasswordCommand MapToCommand(this ForgotPasswordRequest request) =>
        new(request);
}
