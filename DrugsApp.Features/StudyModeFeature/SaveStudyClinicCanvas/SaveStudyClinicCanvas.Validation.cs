using FluentValidation;

namespace DrugsApp.Features.StudyModeFeature.SaveStudyClinicCanvas;

/// <summary>
/// Validator for the canvas save body (shape only; membership is checked by the aggregate).
/// </summary>
public sealed class SaveStudyClinicCanvasRequestValidator : AbstractValidator<SaveStudyClinicCanvasRequest>
{
    private const int MAX_ASSOCIATION_COUNT = 1200;
    private const int MAX_POSITION_COUNT = 80;
    private const double MAX_ABSOLUTE_COORDINATE = 1_000_000;

    /// <summary>
    /// Initializes association and position rules.
    /// </summary>
    public SaveStudyClinicCanvasRequestValidator()
    {
        RuleFor(request => request.Associations)
            .NotNull()
            .WithMessage("Associations are required; send an empty list to remove all edges.");

        When(
            request => request.Associations is not null,
            () =>
            {
                RuleFor(request => request.Associations)
                    .Must(associations => associations.Count <= MAX_ASSOCIATION_COUNT)
                    .WithMessage($"Associations must not exceed {MAX_ASSOCIATION_COUNT} items.")
                    .Must(associations => associations.All(association => association is not null))
                    .WithMessage("Associations must not contain null items.")
                    .Must(associations => associations
                        .Where(association => association is not null)
                        .Select(association => (association.DoctorUserId, association.PatientUserId))
                        .Distinct()
                        .Count() == associations.Count)
                    .WithMessage("Duplicate associations are not allowed.");

                RuleForEach(request => request.Associations)
                    .Must(association => association is null
                        || (association.DoctorUserId != Guid.Empty && association.PatientUserId != Guid.Empty))
                    .WithMessage("Association ids must not be empty.")
                    .Must(association => association is null || association.DoctorUserId != association.PatientUserId)
                    .WithMessage("Association doctor and patient ids must differ.");
            });

        RuleFor(request => request.Positions)
            .NotNull()
            .WithMessage("Positions are required; send an empty list when no node moved.");

        When(
            request => request.Positions is not null,
            () =>
            {
                RuleFor(request => request.Positions)
                    .Must(positions => positions.Count <= MAX_POSITION_COUNT)
                    .WithMessage($"Positions must not exceed {MAX_POSITION_COUNT} items.")
                    .Must(positions => positions.All(position => position is not null))
                    .WithMessage("Positions must not contain null items.")
                    .Must(positions => positions
                        .Where(position => position is not null)
                        .Select(position => position.UserId)
                        .Distinct()
                        .Count() == positions.Count)
                    .WithMessage("Duplicate position user ids are not allowed.");

                RuleForEach(request => request.Positions)
                    .Must(position => position is null || position.UserId != Guid.Empty)
                    .WithMessage("Position user id must not be empty.")
                    .Must(position => position is null
                        || (IsValidCoordinate(position.PositionX) && IsValidCoordinate(position.PositionY)))
                    .WithMessage($"Coordinates must be finite numbers within ±{MAX_ABSOLUTE_COORDINATE:N0}.");
            });
    }

    private static bool IsValidCoordinate(double coordinate) =>
        double.IsFinite(coordinate) && Math.Abs(coordinate) <= MAX_ABSOLUTE_COORDINATE;
}
