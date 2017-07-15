using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Mogo.GameData
{
    public partial class GroupData : GameData<GroupData>
    {
        public static readonly string fileName = "xml/GroupData";

        public string star_color { get; set; }

        public string building_color { get; set; }

        public string soldier_color { get; set; }
    }
}