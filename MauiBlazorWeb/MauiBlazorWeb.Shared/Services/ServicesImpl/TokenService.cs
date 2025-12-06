using Blazored.LocalStorage;

public class TokenService
{
    private const string TokenKey = "jwt_token";
    private readonly ILocalStorageService _localStorage;

    public TokenService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    // Save token asynchronously
    public Task SaveTokenAsync(string token) =>
        _localStorage.SetItemAsync(TokenKey, token).AsTask();

    // Get token asynchronously
    public Task<string?> GetTokenAsync() =>
        _localStorage.GetItemAsync<string>(TokenKey).AsTask();

    // Remove token asynchronously
    public Task RemoveTokenAsync() =>
        _localStorage.RemoveItemAsync(TokenKey).AsTask();
}
