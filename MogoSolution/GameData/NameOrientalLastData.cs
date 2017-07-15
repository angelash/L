using System;
using Mogo.Util;

namespace Mogo.GameData
{
    public partial class NameOrientalLastData : GameData<NameOrientalLastData>
    {
        public static readonly string fileName = "xml/NameOrientalLast";

        public NameOrientalLastData()
        {
            name = string.Empty;
        }

        public string name { get; set; }
    }
}