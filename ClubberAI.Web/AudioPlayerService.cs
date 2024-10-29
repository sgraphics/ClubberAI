using System.Reactive.Linq;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

public class AudioPlayerService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ProtectedSessionStorage _sessionStorage;
    private const string PLAYLIST_KEY = "playlist";

    public AudioPlayerService(IJSRuntime jsRuntime, ProtectedSessionStorage sessionStorage)
    {
        _jsRuntime = jsRuntime;
        _sessionStorage = sessionStorage;
    }

    public async Task Play(string stream)
    {
        await _sessionStorage.SetAsync(PLAYLIST_KEY, stream);
    }

    public async Task Pause()
    {
        await _sessionStorage.DeleteAsync(PLAYLIST_KEY);
    }

    public async Task<IObservable<string>> GetPlaylistStream()
    {
        // If not in storage, poll every second until we find it
        var stream = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1))
            .SelectMany(async _ =>
            {
                try
                {
                    var result = await _sessionStorage.GetAsync<string>(PLAYLIST_KEY);
                    return result.Success ? result.Value : null;
                }
                catch
                {
                    return null;
                }
            });

        var result = await _sessionStorage.GetAsync<string>(PLAYLIST_KEY);
        if (result.Success && !string.IsNullOrEmpty(result.Value))
        {
            stream = stream.StartWith(result.Value);
        }

        return stream.DistinctUntilChanged()!;
    }
}
