using DrugsApp.Domain.TestFeature.Entities;

namespace DrugsApp.Features.TestFeature.Shared;

public static class TestMappings
{
    public static TestDto MapToDto(this Test test)
    {
        return new TestDto(
            test.Id,
            test.Attribute1,
            test.Attribute2,
            test.Attribute3,
            test.Attribute4,
            test.CreatedAt,
            test.UpdatedAt);
    }
}

