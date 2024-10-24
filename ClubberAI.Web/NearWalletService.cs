using Microsoft.JSInterop;
using System.Threading.Tasks;

public class NearWalletService
{
    private readonly IJSRuntime _jsRuntime;

    public NearWalletService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
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
        await _jsRuntime.InvokeVoidAsync("nearWallet.signOut");
    }

    public async Task<bool> IsSignedIn()
    {
        return await _jsRuntime.InvokeAsync<bool>("nearWallet.isSignedIn");
    }

    public async Task<string> GetAccountId()
    {
        return await _jsRuntime.InvokeAsync<string>("nearWallet.getAccountId");
    }
}
