using System.Text.Json;
using System.Text.Json.Serialization;
using EFCore.AuditExtensions.Common.Annotations;

namespace EFCore.AuditExtensions.Common.Extensions;

internal static class AuditSerializationExtensions
{
    private static JsonSerializerOptions JsonSerializerOptions
    {
        get
        {
            var settings = new JsonSerializerOptions(JsonSerializerDefaults.General);
            settings.Converters.Add(new JsonStringEnumConverter());
            return settings;
        }
    }

    public static string Serialize(this Audit audit) => JsonSerializer.Serialize(audit, JsonSerializerOptions);

    public static Audit? Deserialize(this string serializedAudit) => JsonSerializer.Deserialize<Audit>(serializedAudit, JsonSerializerOptions);
}