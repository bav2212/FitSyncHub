using FitSyncHub.Xert.Models;

namespace FitSyncHub.Xert.Abstractions;

public interface IXertAuthService
{
    Task<TokenModel> RequestToken(CancellationToken cancellationToken);
}
