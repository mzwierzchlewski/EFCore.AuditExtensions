using System.Data.Common;
using EFCore.AuditableExtensions.Common.Interceptors;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EFCore.AuditableExtensions.SqlServer.Interceptors;

internal class UserContextInterceptor : DbConnectionInterceptor
{
    private readonly IUserProvider _userProvider;

    public UserContextInterceptor(IUserProvider userProvider)
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

        var command = GetDbCommand(connection, user);
        command.ExecuteNonQuery();
    }

    private static DbCommand GetDbCommand(DbConnection connection, string user)
    {
        var command = connection.CreateCommand();
        command.CommandText = "EXEC sp_set_session_context 'user', @User";
        var userParameter = command.CreateParameter();
        userParameter.ParameterName = "@User";
        userParameter.Value = user;
        command.Parameters.Add(userParameter);
        return command;
    }
}