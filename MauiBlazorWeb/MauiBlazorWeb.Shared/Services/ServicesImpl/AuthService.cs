using System.Net.Http.Json;
using Shared.Dtos.DtosImpl;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly TokenService _tokenService;

    public AuthService(HttpClient httpClient, TokenService tokenService)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
    }

    public async Task<bool> RegisterAsync(RegistrationRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<AuthResponseDto?> AuthenticateAsync(AuthRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
        if (!response.IsSuccessStatusCode) return null;
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        if (authResponse != null)
        {
            await _tokenService.SaveTokenAsync(authResponse.Token);
        }
        return authResponse;
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
            await _tokenService.SaveTokenAsync(authResponse.Token);
        }
        return authResponse;
    }

    public async Task<bool> CheckIfLoggedInAsync()
    {
        var token = await _tokenService.GetTokenAsync();
        if (string.IsNullOrEmpty(token)) return false;
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var response = await _httpClient.GetAsync("api/auth/is-logged-in");
        return response.IsSuccessStatusCode;
    }
}
