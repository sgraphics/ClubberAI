namespace ClubberAI.Web;

public class PartyApiClient
{
	private string _apiService;

	public PartyApiClient(HttpClient httpClient, IConfiguration configuration)
	{
		_apiService = configuration["services:apiservice:https:0"];
	}

	public string GetApiUrl()
	{
		return _apiService;
	}
}
public class TokenApiClient
{
	private readonly HttpClient _client;
	private string _tokenService;

	public TokenApiClient(IConfiguration configuration, HttpClient client)
	{
		_client = client;
		_tokenService = configuration["services:token:https:0"] ?? configuration["services:token:http:0"];
	}

	public HttpClient GetClient() => _client;

	public string GetTokenApiUrl()
	{
		return _tokenService;
	}
}