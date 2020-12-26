using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DangerZoneHackerTracker
{
	public static class ColorExtensions
	{
        public static Color Lerp(this ref Color left, Color right, double amount)
        {
            return Color.FromArgb(
                a: (byte)(left.A + (right.A - left.A) * amount),
                r: (byte)(left.R + (right.R - left.R) * amount),
                g: (byte)(left.G + (right.G - left.G) * amount),
                b: (byte)(left.B + (right.B - left.B) * amount));
        }

        public static Color Lerp(Color left, Color right, double amount)
        {
            return Color.FromArgb(
                a: (byte)(left.A + (right.A - left.A) * amount),
                r: (byte)(left.R + (right.R - left.R) * amount),
                g: (byte)(left.G + (right.G - left.G) * amount),
                b: (byte)(left.B + (right.B - left.B) * amount));
        }
    }
}
