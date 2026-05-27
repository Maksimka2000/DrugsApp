using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceFeature.Repositories;
using DrugsApp.Domain.ActiveSubstanceGroupFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceGroupFeature.Repositories;
using DrugsApp.Domain.FoodFeature.Entities;
using DrugsApp.Domain.FoodFeature.Repositories;
using DrugsApp.Domain.InteractionFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Enums;
using DrugsApp.Domain.InteractionFeature.Repositories;
using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Domain.SupplementFeature.Repositories;
using DrugsApp.Features.InteractionFeature.ApplicationServices;
using DrugsApp.Features.InteractionFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.InteractionFeature.CreateInteraction;



internal sealed class CreateInteractionHandler(
    IInteractionRepository _interactionRepository,
    IActiveSubstanceRepository _activeSubstanceRepository,
    IActiveSubstanceGroupRepository _activeSubstanceGroupRepository,
    IFoodRepository _foodRepository,
    IFoodGroupRepository _foodGroupRepository,
    ISupplementRepository _supplementRepository,
    ISupplementGroupRepository _supplementGroupRepository,
    IInteractionWriteApplicationService _interactionWriteApplicationService)
    : IRequestHandler<CreateInteractionCommand, ErrorOr<CreatedInteractionResponse>>
{

    private sealed record PartyResolutionData(
        InteractionPartyKey PartyKey, // InteractionType: PartyType, Guid: PartyId
        InteractionQueryFoundDto ResolvedDto, // InteractionType: Type, Guid: Id, string: Label, IReadOnlyCollection<IdLabelDto (Guid: Id, string: Label)>? Members 
        ShortInteractionType AtomicType,
        IReadOnlyCollection<Guid> AtomicIds);

    public async Task<ErrorOr<CreatedInteractionResponse>> Handle(
        CreateInteractionCommand command,
        CancellationToken cancellationToken)
    {
        // resolve the left party
        ErrorOr<PartyResolutionData> leftResolutionResult = await ResolvePartyAsync(
            command.Request.Left,
            cancellationToken);
        if (leftResolutionResult.IsError)
        {
            return leftResolutionResult.Errors;
        }
        PartyResolutionData leftResolution = leftResolutionResult.Value;

        // resolve the right party
        ErrorOr<PartyResolutionData> rightResolutionResult = await ResolvePartyAsync(
            command.Request.Right,
            cancellationToken);
        if (rightResolutionResult.IsError)
        {
            return rightResolutionResult.Errors;
        }
        PartyResolutionData rightResolution = rightResolutionResult.Value;

        // get the severity
        InteractionSeverity? severity = await _interactionRepository.GetInteractionSeverityByIdAsync(
            command.Request.SeverityId,
            cancellationToken);
        if (severity is null)
        {
            return Error.NotFound(
                description: "Interaction severity not found.",
                metadata: new Dictionary<string, object>
                {
                    ["resourceType"] = "interactionSeverity",
                    ["resourceId"] = command.Request.SeverityId.ToString()
                });
        }

        // canonicalize the pair
        (InteractionPartyKey firstKey, InteractionPartyKey secondKey) = CanonicalizePair(leftResolution.PartyKey, rightResolution.PartyKey);

        // check if the interaction already exists
        Interaction? existing = await _interactionRepository.GetByCanonicalPairAsync(
            firstKey,
            secondKey,
            cancellationToken);
        if (existing is not null)
        {
            return Error.Conflict(
                description: "Interaction already exists for the provided parties.",
                metadata: new Dictionary<string, object>
                {
                    ["existingInteractionId"] = existing.Id.ToString()
                });
        }

        // create the interaction
        var input = new CreateInteractionWorkflowInput(
            InteractionSeverityId: command.Request.SeverityId,
            DefaultText: command.Request.DefaultText,
            StomachText: command.Request.StomachText,
            LeftParty: leftResolution.PartyKey,
            RightParty: rightResolution.PartyKey,
            LeftAtomicType: leftResolution.AtomicType,
            LeftAtomicIds: leftResolution.AtomicIds,
            RightAtomicType: rightResolution.AtomicType,
            RightAtomicIds: rightResolution.AtomicIds);

        ErrorOr<CreateInteractionWorkflowOutput> createResult = await _interactionWriteApplicationService.CreateAsync(
            input,
            cancellationToken);
        if (createResult.IsError)
        {
            return createResult.Errors;
        }

        CreateInteractionWorkflowOutput interactionDto = createResult.Value;

        return new CreatedInteractionResponse(
            InteractionId: interactionDto.InteractionId,
            Left: leftResolution.ResolvedDto,
            Right: rightResolution.ResolvedDto,
            Severity: severity.MapToDto(),
            DefaultText: interactionDto.DefaultText,
            StomachText: interactionDto.StomachText);
        
    }

    private async Task<ErrorOr<PartyResolutionData>> ResolvePartyAsync(
        InteractionSideRequest side,
        CancellationToken cancellationToken)
    {
        InteractionType type = side.Type;
        Guid id = side.Id;

        switch (type)
        {
            case InteractionType.Substance:
            {
                ActiveSubstance? substance = await _activeSubstanceRepository.GetByIdAsync(id, cancellationToken);
                if (substance is null)
                {
                    return Error.NotFound(
                        description: "Active substance not found.",
                        metadata: new Dictionary<string, object>
                        {
                            ["resourceType"] = "activeSubstance",
                            ["resourceId"] = id.ToString()
                        });
                }

                return new PartyResolutionData(
                    new InteractionPartyKey(InteractionType.Substance, substance.Id),
                    new InteractionQueryFoundDto(InteractionType.Substance, substance.Id, substance.ActiveSubstanceName, null),
                    ShortInteractionType.Substance,
                    new[] { substance.Id });
            }
            case InteractionType.SubstanceGroup:
            {
                ActiveSubstanceGroup? group = await _activeSubstanceGroupRepository.GetByIdAsync(id, cancellationToken);
                if (group is null)
                {
                    return Error.NotFound(
                        description: "Active substance group not found.",
                        metadata: new Dictionary<string, object>
                        {
                            ["resourceType"] = "activeSubstanceGroup",
                            ["resourceId"] = id.ToString()
                        });
                }

                var memberIds = group.ActiveSubstances.Select(member => member.Id).ToList();
                var members = group.ActiveSubstances
                    .Select(member => new IdLabelDto(member.Id, member.ActiveSubstanceName))
                    .ToList();

                return new PartyResolutionData(
                    new InteractionPartyKey(InteractionType.SubstanceGroup, group.Id),
                    new InteractionQueryFoundDto(InteractionType.SubstanceGroup, group.Id, group.Name, members),
                    ShortInteractionType.Substance,
                    memberIds);
            }
            case InteractionType.Food:
            {
                Food? food = await _foodRepository.GetByIdAsync(id, cancellationToken);
                if (food is null)
                {
                    return Error.NotFound(
                        description: "Food not found.",
                        metadata: new Dictionary<string, object>
                        {
                            ["resourceType"] = "food",
                            ["resourceId"] = id.ToString()
                        });
                }

                return new PartyResolutionData(
                    new InteractionPartyKey(InteractionType.Food, food.Id),
                    new InteractionQueryFoundDto(InteractionType.Food, food.Id, food.Name, null),
                    ShortInteractionType.Food,
                    new[] { food.Id });
            }
            case InteractionType.FoodGroup:
            {
                FoodGroup? group = await _foodGroupRepository.GetByIdAsync(id, cancellationToken);
                if (group is null)
                {
                    return Error.NotFound(
                        description: "Food group not found.",
                        metadata: new Dictionary<string, object>
                        {
                            ["resourceType"] = "foodGroup",
                            ["resourceId"] = id.ToString()
                        });
                }

                var memberIds = group.Foods.Select(member => member.Id).ToList();
                var members = group.Foods
                    .Select(member => new IdLabelDto(member.Id, member.Name))
                    .ToList();

                return new PartyResolutionData(
                    new InteractionPartyKey(InteractionType.FoodGroup, group.Id),
                    new InteractionQueryFoundDto(InteractionType.FoodGroup, group.Id, group.Name, members),
                    ShortInteractionType.Food,
                    memberIds);
            }
            case InteractionType.Supplement:
            {
                Supplement? supplement = await _supplementRepository.GetByIdAsync(id, cancellationToken);
                if (supplement is null)
                {
                    return Error.NotFound(
                        description: "Supplement not found.",
                        metadata: new Dictionary<string, object>
                        {
                            ["resourceType"] = "supplement",
                            ["resourceId"] = id.ToString()
                        });
                }

                return new PartyResolutionData(
                    new InteractionPartyKey(InteractionType.Supplement, supplement.Id),
                    new InteractionQueryFoundDto(InteractionType.Supplement, supplement.Id, supplement.Name, null),
                    ShortInteractionType.Supplement,
                    new[] { supplement.Id });
            }
            case InteractionType.SupplementGroup:
            {
                SupplementGroup? group = await _supplementGroupRepository.GetByIdAsync(id, cancellationToken);
                if (group is null)
                {
                    return Error.NotFound(
                        description: "Supplement group not found.",
                        metadata: new Dictionary<string, object>
                        {
                            ["resourceType"] = "supplementGroup",
                            ["resourceId"] = id.ToString()
                        });
                }

                var memberIds = group.Supplements.Select(member => member.Id).ToList();
                var members = group.Supplements
                    .Select(member => new IdLabelDto(member.Id, member.Name))
                    .ToList();

                return new PartyResolutionData(
                    new InteractionPartyKey(InteractionType.SupplementGroup, group.Id),
                    new InteractionQueryFoundDto(InteractionType.SupplementGroup, group.Id, group.Name ?? string.Empty, members),
                    ShortInteractionType.Supplement,
                    memberIds);
            }
            default:
                return Error.Validation(description: "Unsupported interaction party type.");
        }
    }

    private static (InteractionPartyKey First, InteractionPartyKey Second) CanonicalizePair(
        InteractionPartyKey left,
        InteractionPartyKey right)
    {
        bool keepOrder = left.PartyType < right.PartyType ||
            left.PartyType == right.PartyType && left.PartyId.CompareTo(right.PartyId) <= 0;

        return keepOrder ? (left, right) : (right, left);
    }


}
