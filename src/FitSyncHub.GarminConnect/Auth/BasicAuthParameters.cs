using FitSyncHub.GarminConnect.Auth.Abstractions;

namespace FitSyncHub.GarminConnect.Auth;

internal class BasicAuthParameters : IAuthParameters
{
    private readonly string _email;
    private readonly string _password;
    private readonly IUserAgent _userAgent;

    public string UserAgent => _userAgent.New;
    public string Domain => "garmin.com";
    public string? Cookies { get; set; }
    public string? Csrf { get; set; }

    public BasicAuthParameters(string email, string password) : this(email, password, new StaticUserAgent())
    {
    }

    public BasicAuthParameters(string email, string password, IUserAgent userAgent)
    {
        if (string.IsNullOrEmpty(email))
        {
            throw new ArgumentException(email, nameof(email));
        }

        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentException(password, nameof(password));
        }

        _email = email;
        _password = password;
        _userAgent = userAgent;
    }

    public virtual IReadOnlyDictionary<string, string> GetHeaders()
    {
        var headers = new Dictionary<string, string>
        {
            { "User-Agent", UserAgent },
            { "origin", $"https://sso.{Domain}" }
        };

        if (!string.IsNullOrEmpty(Cookies))
        {
            headers.Add("cookie", Cookies);
        }

        return headers;
    }

    public virtual IReadOnlyDictionary<string, string> GetFormParameters()
    {
        var data = new Dictionary<string, string>
        {
            { "embed", "true" },
            { "username", _email },
            { "password", _password }
        };

        if (!string.IsNullOrEmpty(Csrf))
        {
            data.Add("_csrf", Csrf);
        }

        return data;
    }

    public virtual IReadOnlyDictionary<string, string> GetQueryParameters()
    {
        return new Dictionary<string, string>
        {
            { "id", "gauth-widget" },
            { "embedWidget", "true" },
        };
    }
}
