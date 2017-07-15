using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Mogo.GameData
{
    public partial class MapData : GameData<MapData>
    {
        public static readonly string fileName = "xml/MapData";


        public List<int> size { get; set; }

        public List<int> starId { get; set; }

        public List<int> positionX { get; set; }

        public List<int> positionY { get; set; }

    }
}