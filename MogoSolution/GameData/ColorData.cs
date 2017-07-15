using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Mogo.GameData
{
    public partial class ColorData : GameData<ColorData>
    {
        public static readonly string fileName = "xml/ColorData";

        public Color FontColor { get; set; }

        public Color OutlineColor { get; set; }
    }
}