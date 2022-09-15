using System.Data.Common;
using EFCore.AuditExtensions.Common.Interceptors;

namespace EFCore.AuditExtensions.SqlServer.Interceptors;

internal class UserContextInterceptor : BaseUserContextInterceptor
{
    public UserContextInterceptor(IUserProvider userProvider) : base(userProvider)
    { }

    protected override void SetUserContext(DbConnection connection, string user)
    {
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