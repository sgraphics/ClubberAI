namespace ClubberAI.Web;

public class PartyApiClient
{
    private string _apiService;
    private string _tokenService;

    public PartyApiClient(HttpClient httpClient, IConfiguration configuration)
    {
	    _apiService = configuration["services:apiservice:https:0"];
	    _tokenService = configuration["services:token:https:0"] ?? configuration["services:token:http:0"];
	}

    public string GetApiUrl()
    {
	    return _apiService;
	}

    public string GetTokenApiUrl()
    {
	    return _tokenService;
    }
}