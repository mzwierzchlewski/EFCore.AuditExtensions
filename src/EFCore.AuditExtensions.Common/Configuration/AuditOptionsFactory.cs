namespace EFCore.AuditExtensions.Common.Configuration;

internal static class AuditOptionsFactory
{
    public static AuditOptions<T> GetConfiguredAuditOptions<T>(Action<AuditOptions<T>>? configureOptions) where T : class
    {
        var options = new AuditOptions<T>();
        configureOptions?.Invoke(options);

        return options;
    }
}