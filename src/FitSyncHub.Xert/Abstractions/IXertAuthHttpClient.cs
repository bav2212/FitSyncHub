using FitSyncHub.Xert.Models.Responses;

namespace FitSyncHub.Xert.Abstractions;

public interface IXertAuthHttpClient
{
    Task<XertTokenResponse> ObtainTokenAsync(CancellationToken cancellationToken);
    Task<XertTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
}
