namespace EFCore.AuditExtensions.Common.Interceptors;

public interface IUserProvider
{
    string GetCurrentUser();
}