using Shared.Dtos.DtosImpl;

namespace MauiBlazorWeb.Shared.Singletons.SingletonsImpl
{
    public sealed class AuthResponseSingleton
    {
        private AuthResponseDto? _authResponse;
        public event Action? OnChange;
        private void NotifyStateChanged() => OnChange?.Invoke();

        public AuthResponseDto? AuthResponse
        {
            get => _authResponse;
            set
            {
                _authResponse = value;
                NotifyStateChanged();
            }
        }
    }
}