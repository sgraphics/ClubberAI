using System.Reactive.Linq;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Threading.Tasks;

public class NearWalletService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ProtectedSessionStorage _sessionStorage;
    private const string WALLET_KEY = "walletId";

    public NearWalletService(IJSRuntime jsRuntime, ProtectedSessionStorage sessionStorage)
    {
        _jsRuntime = jsRuntime;
        _sessionStorage = sessionStorage;
    }

    public async Task InitNear()
    {
        await _jsRuntime.InvokeVoidAsync("nearWallet.initNear");
    }

    public async Task SignIn()
    {
        await _jsRuntime.InvokeVoidAsync("nearWallet.signIn");
    }

    public async Task SignOut()
    {
        try 
        {
            await _jsRuntime.InvokeVoidAsync("nearWallet.signOut");
            await _sessionStorage.DeleteAsync(WALLET_KEY);
        }
        catch (Exception ex)
        {
            // Consider logging the error
            Console.WriteLine($"Error during sign out: {ex.Message}");
        }
    }

    public async Task<bool> IsSignedIn()
    {
        return await _jsRuntime.InvokeAsync<bool>("nearWallet.isSignedIn");
    }

    public async Task<string> GetAccountId()
    {
        var accountId = await _jsRuntime.InvokeAsync<string>("nearWallet.getAccountId");
        if (!string.IsNullOrEmpty(accountId))
        {
            await _sessionStorage.SetAsync(WALLET_KEY, accountId);
        }
        return accountId;
    }

    public async Task<IObservable<string>> GetAccountIdOrWait()
    {
        // First try to get from storage
        try
        {
            var result = await _sessionStorage.GetAsync<string>(WALLET_KEY);
            if (result.Success && !string.IsNullOrEmpty(result.Value))
            {
                return Observable.Return(result.Value);
            }
        }
        catch { }

        // If not in storage, poll every second until we find it
        return Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1))
            .SelectMany(async _ =>
            {
                try
                {
                    var result = await _sessionStorage.GetAsync<string>(WALLET_KEY);
                    return result.Success ? result.Value : null;
                }
                catch
                {
                    return null;
                }
            })
            .Where(x => !string.IsNullOrEmpty(x))
            .Take(1);
    }

    public async Task<string> GetTokenBalance()
    {
        return await _jsRuntime.InvokeAsync<string>("nearWallet.getTokenBalance");
    }

    public async Task StakeClub()
	{
		await _jsRuntime.InvokeVoidAsync("nearWallet.stakeClub");
	}
}
