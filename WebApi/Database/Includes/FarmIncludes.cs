using System.Linq.Expressions;
using WebApi.Models.ModelsImpl;

namespace WebApi.Database.Includes
{
    public class FarmIncludes
    {
        public static readonly Expression<Func<Farm, object?>>[] Default = new Expression<Func<Farm, object?>>[]
        {
            r => r.Contact,
            r => r.Address,
            r => r.SaveChickenAction,

        };
    }
}
