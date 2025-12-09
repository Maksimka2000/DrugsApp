using DrugsApp.Domain.TestFeature.Entities;
using DrugsApp.Features.TestFeature.Shared;
using ErrorOr;

namespace DrugsApp.Features.TestFeature.CreateTest;

internal static class CreateTestMapping
{
    public static CreateTestCommand MapToCommand(this CreateTestRequest request)
    {
        return new CreateTestCommand(
            request.Attribute1,
            request.Attribute2,
            request.Attribute3,
            request.Attribute4);
    }

    public static Test MapToEntity(this CreateTestCommand command)
    {
        return Test.Create(
            command.Attribute1,
            command.Attribute2,
            command.Attribute3,
            command.Attribute4);
    }

    public static CreatedTestResponse MapToResponse(this Test test)
    {
        TestDto dto = test.MapToDto();

        return new CreatedTestResponse(
            dto.Id,
            dto.Attribute1,
            dto.Attribute2,
            dto.Attribute3,
            dto.Attribute4,
            dto.CreatedAt,
            dto.UpdatedAt);
    }
}

