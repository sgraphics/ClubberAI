using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClubberAI.ServiceDefaults.Model;
using Microsoft.Extensions.Configuration;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ClubberAI.ServiceDefaults.Services;

public class MusicService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private string? _pat;
    private object _lock = new object();

    public MusicService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _httpClient.BaseAddress = new Uri("https://api-b2b.mubert.com/v2/");
    }

    public async Task<string> InitializeMubertAsync()
    {
        if (_pat != null)
            return _pat;

        var request = new MubertAccessRequest
        {
            Params = new AccessParams
            {
                Email = _configuration["Mubert"].Split(",")[0],
                License = _configuration["Mubert"].Split(",")[1],
                Token = _configuration["Mubert"].Split(",")[2]
            }
        };

        var response = await _httpClient.PostAsJsonAsync("GetServiceAccess", request);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<MubertAccessResponse>();
        if (result?.Status != 1)
        {
            throw new Exception("Failed to initialize Mubert");
        }

        _pat = result.Data.Pat;
        return _pat;
    }

    public async Task<GetChannelsResponse> GetChannelsAsync()
    {
        if (string.IsNullOrEmpty(_pat))
        {
            await InitializeMubertAsync();
        }

        var request = new MubertGetPlayMusicRequest
        {
            Params = new MubertParams { Pat = _pat }
        };

        var response = await _httpClient.PostAsJsonAsync("GetPlayMusic", request);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<GetChannelsResponse>();
        return result ?? throw new Exception("Failed to get channels");
    }

    public async Task<string> GetChannelsForAi()
    {
        var channels = await GetChannelsAsync();
        var sb = new StringBuilder();

        foreach (var category in channels.data.categories.Where(x => x.name != "Countries"))
        {
            sb.AppendLine($" - {category.name}");

            foreach (var group in category.groups)
            {
                sb.AppendLine($"  - {group.name}");

                foreach (var channel in group.channels)
                {
                    sb.AppendLine($"   - {channel.playlist}: {channel.name} {channel.emoji}");
                }
            }
        }
        return sb.ToString();
    }
}

// DTOs
public record MubertAccessRequest
{
    [JsonPropertyName("method")]
    public string Method => "GetServiceAccess";

    [JsonPropertyName("params")]
    public AccessParams Params { get; init; } = null!;
}

public record MubertGetPlayMusicRequest
{
    [JsonPropertyName("method")]
    public string Method => "GetPlayMusic";

    [JsonPropertyName("params")]
    public MubertParams Params { get; init; } = null!;
}

public enum MubertIntensity
{
    Low,
    Medium,
    High
}

public record MubertParams
{
    [JsonPropertyName("pat")]
    public string Pat { get; set; }
}

public record AccessParams
{
    [JsonPropertyName("email")]
    public string Email { get; init; } = null!;
    
    [JsonPropertyName("license")]
    public string License { get; init; } = null!;
    
    [JsonPropertyName("token")]
    public string Token { get; init; } = null!;
}

public record MubertAccessResponse
{
    [JsonPropertyName("status")]
    public int Status { get; init; }
    
    [JsonPropertyName("data")]
    public AccessResponseData Data { get; init; } = null!;
}

public record AccessResponseData
{
    [JsonPropertyName("pat")]
    public string Pat { get; init; } = null!;
}

// Add other DTOs for channels response and intensity request...