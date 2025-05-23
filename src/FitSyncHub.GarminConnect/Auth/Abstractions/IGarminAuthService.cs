using FitSyncHub.GarminConnect.Auth.Models;

namespace FitSyncHub.GarminConnect.Auth.Abstractions;

public interface IGarminAuthService
{
    Task<GarminLoginResult> Login(CancellationToken cancellationToken);
    Task<GarminAuthenticationResult> ResumeLogin(string mfaCode, CancellationToken cancellationToken);
}
