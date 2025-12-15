using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

public class ItemWithBrowserFiles<T> where T : IEntityWithFiles
{
    [ValidateComplexType]
    public T Item { get; set; } = default!;
    public List<IBrowserFile> Files { get; set; }

    public ItemWithBrowserFiles()
    {
        Files = new List<IBrowserFile>();
    }
}
