using System;

namespace DrugsApp.Features.AllergiesFeature.CheckFoodCustomNameAvailability;

public static class CheckFoodCustomNameAvailabilityMapping
{
    public static CheckFoodCustomNameAvailabilityQuery MapToQuery(
        this CheckFoodCustomNameAvailabilityRequest request, Guid userId) =>
        new(userId, request.CustomFoodName);
}
