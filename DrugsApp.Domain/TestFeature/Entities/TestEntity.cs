using DrugsApp.Domain.Common;

namespace DrugsApp.Domain.TestFeature.Entities;

public sealed class Test : BaseEntity
{
    private Test()
    {
    }

    public string Attribute1 { get; private set; } = string.Empty;
    public int? Attribute2 { get; private set; }
    public decimal? Attribute3 { get; private set; }
    public bool Attribute4 { get; private set; }

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

