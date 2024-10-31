namespace ClubberAI.Web;

public class PartyApiClient
{
    private string _cachedBaseUrl;

    public PartyApiClient(HttpClient httpClient, IConfiguration configuration)
    {
	    _cachedBaseUrl = configuration["services:apiservice:https:0"];
    }

    public string GetBaseUrl()
    {
	    return _cachedBaseUrl;
    }
}