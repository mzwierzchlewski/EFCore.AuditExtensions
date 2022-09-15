using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EFCore.AuditExtensions.Common.Extensions;

internal static class ExpressionExtensions
{
    public static (string name, Type type) GetAccessedPropertyNameAndType<T>(this Expression<Func<T, object?>> expression)
    {
        var memberInfos = expression.GetMemberAccessList();
        if (memberInfos.Count != 1 && memberInfos[0].MemberType != MemberTypes.Property)
        {
            return (string.Empty, typeof(object));
        }

        var name = memberInfos[0].Name;
        var type = ((PropertyInfo)memberInfos[0]).PropertyType;

        return (name, type);
    }
}