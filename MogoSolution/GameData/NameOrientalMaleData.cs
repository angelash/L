using System;
using Mogo.Util;

namespace Mogo.GameData
{
    public partial class NameOrientalMaleData : GameData<NameOrientalMaleData>
    {
        public static readonly string fileName = "xml/NameOrientalMale";

        public NameOrientalMaleData()
        {
            name = string.Empty;
        }

        public string name { get; set; }
    }
}