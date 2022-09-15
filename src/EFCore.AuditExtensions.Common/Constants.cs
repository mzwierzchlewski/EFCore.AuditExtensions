namespace EFCore.AuditExtensions.Common;

internal static class Constants
{
    public static string AnnotationPrefix => "AuditExtensions";

    public static string AuditTableNameSuffix => "_Audit";

    public static string AuditTriggerPrefix => "Audit_";

    public static class AuditTableColumnNames
    {
        public static string OldData = nameof(OldData);
        public static string NewData = nameof(NewData);
        public static string OperationType = nameof(OperationType);
        public static string Timestamp = nameof(Timestamp);
        public static string User = nameof(User);
    }
}