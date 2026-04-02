using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiBlazorWeb.Shared.Classes
{
    public class MapArrow
    {
        public double LatitudeFrom { get; set; }
        public double LongitudeFrom { get; set; }
        public double LatitudeTo { get; set; }
        public double LongitudeTo { get; set; }
        public double DistanceInMeters { get; set; }
    }
}
