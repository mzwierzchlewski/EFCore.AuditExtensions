using EFCore.AuditableExtensions.Common.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EFCore.AuditableExtensions.Common.Extensions;

internal static class AuditSerializationExtensions
{
    private static JsonSerializerSettings JsonSerializerSettings
    {
        get
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
            };
            settings.Converters.Add(new StringEnumConverter());
            return settings;
        }
    }

    public static string Serialize(this IAudit audit) => JsonConvert.SerializeObject(audit, JsonSerializerSettings);

    public static IAudit? Deserialize(this string serializedAudit) => JsonConvert.DeserializeObject<SimpleAudit>(serializedAudit, JsonSerializerSettings);
}