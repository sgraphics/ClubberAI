namespace ClubberAI.Web;

public class PartyApiClient
{
    private readonly HttpClient _httpClient;
    private string? _cachedBaseUrl;

    public PartyApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public string GetBaseUrl()
    {
        if (_cachedBaseUrl != null)
        {
            return _cachedBaseUrl;
        }
        string GetBaseUrlFromRequest()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/");
            _ = _httpClient.SendAsync(request).ConfigureAwait(false);
            return (request.RequestUri?.ToString() ?? string.Empty).Trim('/');
        }
        for (int i = 0; i < 2; i++)
        {
            var baseUrl = GetBaseUrlFromRequest();
            
            if (!string.IsNullOrEmpty(baseUrl) && !baseUrl.Contains("+"))
            {
                _cachedBaseUrl = baseUrl;
                return _cachedBaseUrl;
            }
        }
        return GetBaseUrlFromRequest();
    }
}