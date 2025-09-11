using FitSyncHub.Common.Services;
using FitSyncHub.Functions.JsonSerializerContexts;
using FitSyncHub.Xert.Abstractions;
using FitSyncHub.Xert.Models;
using FitSyncHub.Xert.Models.Responses;

namespace FitSyncHub.Xert.Services;

internal class XertAuthService : IXertAuthService
{
    private readonly string _authenticationCacheKey = "xert-oauth2-token";

    private readonly IDistributedCacheService _distributedCacheService;
    private readonly IXertAuthHttpClient _xertAuthHttpClient;

    public XertAuthService(
        IDistributedCacheService distributedCacheService,
        IXertAuthHttpClient xertAuthHttpClient)
    {
        _distributedCacheService = distributedCacheService;
        _xertAuthHttpClient = xertAuthHttpClient;
    }

    public async Task<TokenModel> RequestToken(CancellationToken cancellationToken)
    {
        var savedTokenModel = await GetCachedTokenModel(cancellationToken);

        if (savedTokenModel == null)
        {
            var tokenResponse = await _xertAuthHttpClient.ObtainTokenAsync(cancellationToken);
            var tokenModel = ConvertToTokenModel(tokenResponse);

            await CacheTokenModel(tokenModel, cancellationToken);

            return tokenModel;
        }

        if (DateTimeOffset.FromUnixTimeSeconds(savedTokenModel.ExpiresAt) >= DateTimeOffset.UtcNow)
        {
            return savedTokenModel;
        }
        else
        {
            var tokenResponse = await _xertAuthHttpClient.RefreshTokenAsync(savedTokenModel.RefreshToken, cancellationToken);
            var tokenModel = ConvertToTokenModel(tokenResponse);

            await CacheTokenModel(tokenModel, cancellationToken);

            return tokenModel;
        }

        throw new NotImplementedException();
    }

    private static TokenModel ConvertToTokenModel(XertTokenResponse tokenResponse)
    {
        return new TokenModel
        {
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            ExpiresIn = tokenResponse.ExpiresIn,
            ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn).ToUnixTimeSeconds(),
            TokenType = tokenResponse.TokenType,
            Scope = tokenResponse.Scope,
        };
    }

    private async Task CacheTokenModel(TokenModel tokenModel, CancellationToken cancellationToken)
    {
        await _distributedCacheService.SetValueAsync(
           _authenticationCacheKey,
           tokenModel,
           XertSerializerContext.Default.TokenModel,
           cancellationToken);
    }

    public async Task<TokenModel?> GetCachedTokenModel(CancellationToken cancellationToken)
    {
        return await _distributedCacheService.GetValueAsync(
           _authenticationCacheKey,
           XertSerializerContext.Default.TokenModel,
           cancellationToken);
    }
}
