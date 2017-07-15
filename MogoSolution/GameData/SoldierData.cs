using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Mogo.GameData
{
    public partial class SoldierData : GameData<SoldierData>
    {
        public static readonly string fileName = "xml/SoldierData";

        public int energy { get; set; }

        public int attack { get; set; }

        public int move { get; set; }

        public int name { get; set; }

        public int attack_priority { get; set; }

        public List<int> attack_object { get; set; }

        public int score { get; set; }

        public string icon { get; set; }

        public int float_color { get; set; }

        public int building_priority { get; set; }
    }
}