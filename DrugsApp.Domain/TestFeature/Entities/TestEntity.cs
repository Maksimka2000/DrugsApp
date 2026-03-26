using DrugsApp.Domain.Common;

namespace DrugsApp.Domain.TestFeature.Entities;

/// <summary>
/// Test aggregate used for development and reference endpoints.
/// </summary>
public sealed class Test : BaseEntity
{
    private Test()
    {
    }

    /// <summary>
    /// The first attribute of the test.
    /// </summary>
    public string Attribute1 { get; private set; } = string.Empty;

    /// <summary>
    /// The second attribute of the test.
    /// </summary>
    public int? Attribute2 { get; private set; }

    /// <summary>
    /// The third attribute of the test.
    /// </summary>
    public decimal? Attribute3 { get; private set; }

    /// <summary>
    /// The fourth attribute of the test.
    /// </summary>
    public bool Attribute4 { get; private set; }

    /// <summary>
    /// Creates a new <see cref="Test"/> aggregate.
    /// </summary>
    /// <param name="attribute1">First attribute.</param>
    /// <param name="attribute2">Optional second attribute.</param>
    /// <param name="attribute3">Optional third attribute.</param>
    /// <param name="attribute4">Optional fourth attribute.</param>
    /// <returns>A new <see cref="Test"/> aggregate.</returns>
    public static Test Create(
        string attribute1,
        int? attribute2,
        decimal? attribute3,
        bool? attribute4 = null)
    {
        string normalizedAttribute1 = attribute1.Trim();
        decimal? normalizedAttribute3 = null;
        if (attribute3.HasValue)
        {
            normalizedAttribute3 = decimal.Round(attribute3.Value, 2, MidpointRounding.AwayFromZero);
        }

        var test = new Test
        {
            Attribute1 = normalizedAttribute1,
            Attribute2 = attribute2,
            Attribute3 = normalizedAttribute3,
            Attribute4 = attribute4 ?? false
        };

        return test;
    }
}

