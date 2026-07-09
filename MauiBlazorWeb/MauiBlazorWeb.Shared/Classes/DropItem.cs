using Shared.Dtos.DtosImpl;

namespace MauiBlazorWeb.Shared.Classes
{
    public class DropItem
    {
        public string Name { get; init; } = string.Empty;
        public string Selector { get; set; } = string.Empty;
        public SaveChickenRequestDto saveChickenRequest { get; set; } = default!;
    }
}
