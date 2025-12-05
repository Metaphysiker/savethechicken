namespace WebApi.Factories
{
    public interface IFactory<T>
    {
        T Create();
    }
}
