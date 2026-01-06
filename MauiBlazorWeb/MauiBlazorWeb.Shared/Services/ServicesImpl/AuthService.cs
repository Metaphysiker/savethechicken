using MauiBlazorWeb.Shared.Singletons.SingletonsImpl;
using Shared.Dtos.DtosImpl;
using System.Net.Http.Headers;
using System.Net.Http.Json;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly TokenService _tokenService;
    private readonly AuthResponseSingleton _authResponseSingleton;
    public bool isLoading = true;

    public AuthService(HttpClient httpClient, TokenService tokenService, AuthResponseSingleton authResponseSingleton)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
        _authResponseSingleton = authResponseSingleton;
    }

    public async Task<bool> RegisterAsync(RegistrationRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<AuthResponseDto?> AuthenticateAsync(AuthRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
        if (!response.IsSuccessStatusCode)
        {
            await _tokenService.RemoveTokenAsync();
            _authResponseSingleton.AuthResponse = null;
            return null;
        }
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        if (authResponse != null)
        {
            _authResponseSingleton.AuthResponse = authResponse;
            await _tokenService.SaveTokenAsync(authResponse.Token);
        }
        return authResponse;
    }

    public async Task LogoutAsync()
    {
        _authResponseSingleton.AuthResponse = null;
        await _tokenService.RemoveTokenAsync();
    }

    public async Task<AuthResponseDto?> RefreshTokenAsync()
    {
        var token = await _tokenService.GetTokenAsync();
        if (string.IsNullOrEmpty(token)) return null;
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var response = await _httpClient.GetAsync("api/auth/refresh-token");
        if (!response.IsSuccessStatusCode) return null;
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        if (authResponse != null)
        {
            _authResponseSingleton.AuthResponse = authResponse;
            await _tokenService.SaveTokenAsync(authResponse.Token);
        }
        return authResponse;
    }

    public async Task<bool> CheckIfLoggedInAsyncOld()
    {
        isLoading = true;
        try
        {
            var token = await _tokenService.GetTokenAsync();
            if (string.IsNullOrEmpty(token)) return false;
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync("api/auth/is-logged-in");
            return response.IsSuccessStatusCode;
        }
        finally
        {
            isLoading = false;
        }
    }

    public async Task<bool> CheckIfLoggedInAsync(CancellationToken cancellationToken = default)
    {
        isLoading = true;
        try
        {
            var token = await _tokenService.GetTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
                return false;

            using var request = new HttpRequestMessage(HttpMethod.Get, "api/auth/is-logged-in");
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        finally
        {
            isLoading = false;
        }
    }

}
