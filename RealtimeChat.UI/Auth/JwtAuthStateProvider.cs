using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace RealtimeChat.UI.Auth;

public class JwtAuthStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _jsRuntime;
    private readonly HttpClient _httpClient;

    public JwtAuthStateProvider(IJSRuntime jsRuntime, HttpClient httpClient)
    {
        _jsRuntime = jsRuntime;
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

        if (string.IsNullOrWhiteSpace(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try 
        {
            var userInfo = await _httpClient.GetFromJsonAsync<UserInfo>("/manage/info");
            
            if (userInfo?.Email != null)
            {
                var claims = new[] { new Claim(ClaimTypes.Name, userInfo.Email) };
                var identity = new ClaimsIdentity(claims, "Identity.Application");
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
        }
        catch 
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
        }

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public void NotifyUserLoggedIn(string email)
    {
        var claims = new[] { new Claim(ClaimTypes.Name, email) };
        var identity = new ClaimsIdentity(claims, "Identity.Application");
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity))));
    }

    public void NotifyUserLoggedOut()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
    }

    private class UserInfo 
    { 
        public string? Email { get; set; } 
    }
}