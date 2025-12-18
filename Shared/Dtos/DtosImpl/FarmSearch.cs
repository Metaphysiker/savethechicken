using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Dtos.DtosImpl
{
    public class FarmSearch : ISearchDto
    {
        public List<int>? SaveChickenActionIds { get; set; }
    }
}
