namespace DrugsApp.Features.AccountFeature.ConfirmLoginEmailChange;

internal static class ConfirmLoginEmailChangeMapping
{
    public static ConfirmLoginEmailChangeCommand MapToCommand(this ConfirmLoginEmailChangeRequest request)
    {
        return new ConfirmLoginEmailChangeCommand(request.UserId, request.Token);
    }
}
