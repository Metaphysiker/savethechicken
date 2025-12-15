using System.Linq.Expressions;
using WebApi.Models.ModelsImpl;

namespace WebApi.Database.Includes
{
    public class SaveChickenRequestIncludes
    {
        public static readonly Expression<Func<SaveChickenRequest, object?>>[] Default = new Expression<Func<SaveChickenRequest, object?>>[]
        {
            r => r.Contact,
            r => r.Address,
            r => r.AddressForHandOver,
            r => r.SaveChickenAction,
            r => r.Files
        };
    }
}
