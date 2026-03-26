using DrugsApp.Domain.TestFeature.Entities;

namespace DrugsApp.Features.TestFeature.Shared;

/// <summary>
/// Mapping extensions for test feature models.
/// </summary>
public static class TestMappings
{
    /// <summary>
    /// Maps a domain <see cref="Test"/> to its DTO representation.
    /// </summary>
    /// <param name="test">Domain test aggregate.</param>
    /// <returns>Mapped <see cref="TestDto"/>.</returns>
    public static TestDto MapToDto(this Test test)
    {
        return new TestDto(
            test.Id,
            test.Attribute1,
            test.Attribute2,
            test.Attribute3,
            test.Attribute4);
    }
}

