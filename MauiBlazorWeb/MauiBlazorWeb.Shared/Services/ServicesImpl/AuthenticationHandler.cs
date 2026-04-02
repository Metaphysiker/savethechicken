using System.Net;
using System.Net.Http.Headers;

namespace MauiBlazorWeb.Shared.Services.ServicesImpl;

public class AuthenticationHandler : DelegatingHandler
{
    private readonly TokenService _tokenService;

    public AuthenticationHandler(TokenService tokenService)
    {
        _tokenService = tokenService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Add JWT token to every request if available
        var token = await _tokenService.GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // Send the request
        var response = await base.SendAsync(request, cancellationToken);

        // If we get 401 Unauthorized, clear the token
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await _tokenService.RemoveTokenAsync();
        }

        return response;
    }
}
