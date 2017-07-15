using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Mogo.GameData
{
    public partial class BuildingData : GameData<BuildingData>
    {
        public static readonly string fileName = "xml/BuildingData";

        public int energy { get; set; }

        public int name { get; set; }

        public int attack_priority { get; set; }

        public int building_consume { get; set; }

        public float building_time { get; set; }

        public float production_time { get; set; }

        public List<int> level_up { get; set; }

        public int score { get; set; }

        public int type { get; set; }

        public int scale { get; set; }

        public int soldier_id { get; set; }

        public string icon { get; set; }

        public int float_color { get; set; }

        public int attack { get; set; }
    }
}