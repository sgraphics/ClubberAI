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

        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        _ = _httpClient.SendAsync(request).ConfigureAwait(false);
        _cachedBaseUrl = (request.RequestUri?.ToString() ?? string.Empty).Trim('/');
        return _cachedBaseUrl;
    }
}