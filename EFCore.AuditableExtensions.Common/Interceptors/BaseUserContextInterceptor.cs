using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EFCore.AuditableExtensions.Common.Interceptors;

internal abstract class BaseUserContextInterceptor : DbConnectionInterceptor
{
    private readonly IUserProvider _userProvider;

    protected BaseUserContextInterceptor(IUserProvider userProvider)
    {
        _userProvider = userProvider;
    }

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        var user = _userProvider.GetCurrentUser();
        if (string.IsNullOrEmpty(user))
        {
            return;
        }

        SetUserContext(connection, user);
    }

    public override Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = new())
    {
        ConnectionOpened(connection, eventData);
        return Task.CompletedTask;
    }

    protected abstract void SetUserContext(DbConnection connection, string user);
}