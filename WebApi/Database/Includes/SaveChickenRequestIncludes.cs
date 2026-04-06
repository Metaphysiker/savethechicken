using System.Linq.Expressions;
using WebApi.Models.ModelsImpl;

namespace WebApi.Database.Includes
{
    public class SaveChickenRequestIncludes
    {
        // String-based includes for proper EF Core navigation property chaining
        public static readonly string[] DefaultStrings = new string[]
        {
            "Person.Contact",
            "Person.Address",
            "Person.Address.GeoCoordinate",
            "AddressForHandOver",
            "SaveChickenAction",
            "Files"
        };

        // Keep expression-based for compatibility (use DefaultStrings for searches)
        public static readonly Expression<Func<SaveChickenRequest, object?>>[] Default = new Expression<Func<SaveChickenRequest, object?>>[]
        {
            r => r.Person,
            r => r.Person.Contact,
            r => r.Person.Address,
            r => r.Person.Address.GeoCoordinate,
            r => r.AddressForHandOver,
            r => r.SaveChickenAction,
            r => r.Files
        };
    }
}
