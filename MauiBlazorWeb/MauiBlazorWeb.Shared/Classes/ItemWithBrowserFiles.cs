using Microsoft.AspNetCore.Components.Forms;

public class ItemWithBrowserFiles<T>
{
    public T Item { get; set; } = default!;
    public List<IBrowserFile> Files { get; set; }

    public ItemWithBrowserFiles()
    {
        Files = new List<IBrowserFile>();
    }
}
