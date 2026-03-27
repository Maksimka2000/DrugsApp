using DrugsApp.Domain.Common;
using ErrorOr;

namespace DrugsApp.Domain.TempDrugFeature.Entities;

/// <summary>
/// Represents default source-to-target mapping used in import wizard initialization.
/// </summary>
public sealed class DefaultMappingForColumnsAttribute : BaseEntity
{
    private DefaultMappingForColumnsAttribute() : base()
    {
    }

    /// <summary>
    /// Source column name in imported file (usually Polish label).
    /// </summary>
    public string SourceColumnName { get; private set; } = null!;

    /// <summary>
    /// Target TempDrug field name.
    /// </summary>
    public string MappedFieldName { get; private set; } = null!;

    /// <summary>
    /// Creates default mapping entry.
    /// </summary>
    public static ErrorOr<DefaultMappingForColumnsAttribute> Create(
        string sourceColumnName,
        string mappedFieldName)
    {
        if (string.IsNullOrWhiteSpace(sourceColumnName))
        {
            return Error.Validation(description: "SourceColumnName is required.");
        }

        if (string.IsNullOrWhiteSpace(mappedFieldName))
        {
            return Error.Validation(description: "MappedFieldName is required.");
        }

        return new DefaultMappingForColumnsAttribute
        {
            SourceColumnName = sourceColumnName.Trim(),
            MappedFieldName = mappedFieldName.Trim()
        };
    }
}

