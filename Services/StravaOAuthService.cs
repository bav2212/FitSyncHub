using StravaWebhooksAzureFunctions.Data.Entities;
using StravaWebhooksAzureFunctions.HttpClients.Interfaces;
using StravaWebhooksAzureFunctions.HttpClients.Models.Responses;
using StravaWebhooksAzureFunctions.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace StravaWebhooksAzureFunctions.Services;

public class StravaOAuthService : IStravaOAuthService
{
    private readonly IStravaOAuthHttpClient _stravaOAuthHttpClient;

    public StravaOAuthService(IStravaOAuthHttpClient stravaOAuthHttpClient)
    {
        _stravaOAuthHttpClient = stravaOAuthHttpClient;
    }

    public async Task<TokenResponseModel> RequestToken(long athleteId)
    {
        throw new NotImplementedException();

        //var persistedGrant = await _context.Set<PersistedGrant>()
        //    .AsTracking()
        //    .Where(x => x.AthleteId == athleteId)
        //    .FirstOrDefaultAsync()
        //    ?? throw new NotImplementedException();

        //var timespanSinceEpoch = DateTime.UtcNow - new DateTime(1970, 1, 1);
        //var secondsSinceEpoch = (int)timespanSinceEpoch.TotalSeconds;

        //if (persistedGrant.ExpiresAt >= secondsSinceEpoch)
        //{
        //    return new(persistedGrant.AccessToken);
        //}

        //return await RefreshToken(persistedGrant);
    }

    private async Task<TokenResponseModel> RefreshToken(PersistedGrant persistedGrant)
    {
        throw new NotImplementedException();

        //var refreshTokenResponse = await _stravaOAuthHttpClient.RefreshTokenAsync(persistedGrant.RefreshToken);

        //persistedGrant.AccessToken = refreshTokenResponse.AccessToken;
        //persistedGrant.RefreshToken = refreshTokenResponse.RefreshToken;
        //persistedGrant.ExpiresIn = refreshTokenResponse.ExpiresIn;
        //persistedGrant.ExpiresAt = refreshTokenResponse.ExpiresAt;
        //persistedGrant.TokenType = refreshTokenResponse.TokenType;

        //_context.Update(persistedGrant);
        //await _context.SaveChangesAsync();

        //return new(persistedGrant.AccessToken);
    }
}
