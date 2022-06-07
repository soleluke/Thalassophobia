using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Thalassophobia.Utils
{
    static class CustomDamageColor
    {
        private static Color[] colors = new Color[15];
        private static int numColors = 0;

        public static void Init()
        {
            On.RoR2.DamageColor.FindColor += DamageColor_FindColor;
        }

        private static Color DamageColor_FindColor(On.RoR2.DamageColor.orig_FindColor orig, RoR2.DamageColorIndex colorIndex)
        {
            if (colorIndex >= DamageColorIndex.Count)
            {
                return colors[colorIndex - DamageColorIndex.Count];
            }
            return orig(colorIndex);
        }

        public static DamageColorIndex AddColor(Color color)
        {
            DamageColorIndex index = (DamageColorIndex)(numColors + (int)DamageColorIndex.Count);
            colors[numColors] = color;
            numColors++;
            return index;
        }
    }
}
