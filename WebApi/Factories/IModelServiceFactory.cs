namespace WebApi.Factories
{
    public interface IModelServiceFactory<ModelT> where ModelT : IModel
    {
        ModelT Create();
    }
}
