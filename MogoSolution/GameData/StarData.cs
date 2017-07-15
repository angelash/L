using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Mogo.GameData
{
    public partial class StarData : GameData<StarData>
    {
        public static readonly string fileName = "xml/StarData";

        public int energy { get; set; }

        public int count { get; set; }

        public int radius { get; set; }

        public int range { get; set; }

        public bool belong { get; set; }

        public int score { get; set; }

        public string icon { get; set; }
    }
}