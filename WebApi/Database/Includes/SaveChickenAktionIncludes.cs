using System.Linq.Expressions;
using WebApi.Models.ModelsImpl;

namespace WebApi.Database.Includes
{
    public class SaveChickenActionIncludes
    {
        public static readonly Expression<Func<SaveChickenAction, object?>>[] Default = new Expression<Func<SaveChickenAction, object?>>[]
        {
            r => r.SaveChickenRequests
        };
    }
}
