using System.Linq.Expressions;
using WebApi.Models.ModelsImpl;

namespace WebApi.Database.Includes
{
    public class SaveChickenDriveRequestIncludes
    {
        public static readonly Expression<Func<SaveChickenDriveRequest, object?>>[] Default = new Expression<Func<SaveChickenDriveRequest, object?>>[]
        {
            r => r.Person,
            r => r.Person.Contact,
            r => r.Person.Address,
            r => r.Person.Address.GeoCoordinate,
            r => r.SaveChickenAction,
            r => r.Files,
        };
    }
}
