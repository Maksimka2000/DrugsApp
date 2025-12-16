using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DrugsApp.Infrastructure.Common.Serialization;

/// <summary>
/// Allows Newtonsoft.Json to set private setters when deserializing cached aggregates.
/// </summary>
internal sealed class PrivateResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(
        MemberInfo member,
        MemberSerialization memberSerialization)
    {
        JsonProperty property = base.CreateProperty(member, memberSerialization);

        if (!property.Writable && member is PropertyInfo propertyInfo)
        {
            // return setter if exist specify true if should consider non public setter.
            bool hasPrivateSetter = propertyInfo.GetSetMethod(true) is not null;
            property.Writable = hasPrivateSetter;
        }

        return property;
    }
}

