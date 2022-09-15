using EFCore.AuditExtensions.Common.Annotations.Table;

namespace EFCore.AuditExtensions.Common.Extensions;

internal static class AuditColumnTypeExtensions
{
    private const AuditColumnType DefaultColumnType = AuditColumnType.Text;

    private static readonly (AuditColumnType columnType, Type clrType)[] Mappings =
    {
        (AuditColumnType.Guid, typeof(Guid)),
        (AuditColumnType.DateTime, typeof(DateTime)),
        (AuditColumnType.Number, typeof(int)),
        (AuditColumnType.DecimalNumber, typeof(double)),
        (AuditColumnType.PrecisionNumber, typeof(decimal)),
    };

    private static readonly Type DefaultClrType = typeof(string);

    public static Type GetClrType(this AuditColumnType auditColumnType)
    {
        var mapping = Mappings.Where(m => m.columnType == auditColumnType).Take(1).ToArray();
        return mapping.Any() ? mapping[0].clrType : DefaultClrType;
    }

    public static AuditColumnType GetAuditColumnType(this Type type)
    {
        var mapping = Mappings.Where(m => m.clrType.IsEquivalentTo(type)).Take(1).ToArray();
        return mapping.Any() ? mapping[0].columnType : DefaultColumnType;
    }
}