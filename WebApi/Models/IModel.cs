using Shared.Interfaces;

public interface IModel : IEntityWithId
{
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
}
