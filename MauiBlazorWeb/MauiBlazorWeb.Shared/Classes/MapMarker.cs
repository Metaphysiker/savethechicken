using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiBlazorWeb.Shared.Classes
{
    public class MapMarker
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Info { get; set; } = string.Empty;
    }
}
