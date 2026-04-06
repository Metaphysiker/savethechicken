using System.Linq.Expressions;
using WebApi.Models.ModelsImpl;

namespace WebApi.Database.Includes
{
    public class SaveChickenDriveRequestIncludes
    {
        // String-based includes for proper EF Core navigation property chaining
        public static readonly string[] DefaultStrings = new string[]
        {
            "Person.Contact",
            "Person.Address",
            "Person.Address.GeoCoordinate",
            "SaveChickenAction",
            "Files",
        };

        // Keep expression-based for compatibility (use DefaultStrings for searches)
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
