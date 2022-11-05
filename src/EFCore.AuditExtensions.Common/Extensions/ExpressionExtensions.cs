using System.Linq.Expressions;
using System.Reflection;
using EFCore.AuditExtensions.Common.SharedModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EFCore.AuditExtensions.Common.Extensions;

internal static class ExpressionExtensions
{
    public static IReadOnlyCollection<AuditedEntityKeyProperty> GetKeyProperties<T>(this Expression<Func<T, object?>> expression , IReadOnlyEntityType entityType)
    {
        var memberInfos = expression.GetMemberAccessList().Where(m => m.MemberType == MemberTypes.Property);
        
        var storeObject = StoreObjectIdentifier.Table(entityType.GetTableName()!);
        return memberInfos.Select(entityType.FindProperty).Select(property => new AuditedEntityKeyProperty(property!.GetColumnName(storeObject)!, property!.ClrType.GetAuditColumnType(), property.GetMaxLength())).ToArray();
    }
}