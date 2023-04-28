namespace EFCore.AuditExtensions.Common.Interceptors;

internal class EmptyUserProvider : IUserProvider
{
    public string GetCurrentUser() => string.Empty;
}