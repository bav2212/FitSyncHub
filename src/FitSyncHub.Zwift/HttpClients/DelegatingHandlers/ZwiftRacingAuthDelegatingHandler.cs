using Microsoft.AspNetCore.Http;

namespace FitSyncHub.Zwift.HttpClients.DelegatingHandlers;

public sealed class ZwiftRacingAuthDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ZwiftRacingAuthDelegatingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var context = _httpContextAccessor.HttpContext;
        var cookie = context?.Request.Query["cookie"].ToString();

        if (!string.IsNullOrEmpty(cookie))
        {
            request.Headers.Add("Cookie", cookie);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
