namespace EFCore.AuditableExtensions.Common.Interceptors;

public interface IUserProvider
{
    string GetCurrentUser();
}