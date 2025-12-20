using WebApi.Models.ModelsImpl;

namespace WebApi.Interfaces
{
    public interface IEntityWithFiles
    {
        public int Id { get; set; }
        public List<StoredFile> Files { get; set; }
    }
}
