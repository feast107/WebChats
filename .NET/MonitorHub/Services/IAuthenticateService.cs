using MonitorHub.Models;
namespace MonitorHub.Services
{
    public interface IAuthenticateService
    {
        bool IsAuthenticated(UserIdentity request, out string token);
    }
}
