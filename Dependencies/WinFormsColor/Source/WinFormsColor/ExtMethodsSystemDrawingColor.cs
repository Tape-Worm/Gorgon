using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fetze.WinFormsColor;

public static class ExtMethodsSystemDrawingColor
{
    public static float GetLuminance(this System.Drawing.Color color) => (0.2126f * color.R + 0.7152f * color.G + 0.0722f * color.B) / 255.0f;
    public static float GetHSVHue(this System.Drawing.Color color) => color.GetHue() / 360.0f;
    public static float GetHSVBrightness(this System.Drawing.Color color) => Math.Max(Math.Max(color.R, color.G), color.B) / 255.0f;
    public static float GetHSVSaturation(this System.Drawing.Color color)
    {
        int max = Math.Max(color.R, Math.Max(color.G, color.B));
        int min = Math.Min(color.R, Math.Min(color.G, color.B));

        return (max == 0) ? 0.0f : 1.0f - (1.0f * min / max);
    }

    public static System.Drawing.Color ColorFromHSV(float hue, float saturation, float value)
    {
        hue *= 360.0f;

        var hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        var f = hue / 60 - Math.Floor(hue / 60);

        value *= 255;
        var v = Convert.ToInt32(value);
        var p = Convert.ToInt32(value * (1 - saturation));
        var q = Convert.ToInt32(value * (1 - f * saturation));
        var t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

        switch (hi)
        {
            case 0:
                return System.Drawing.Color.FromArgb(255, v, t, p);
            case 1:
                return System.Drawing.Color.FromArgb(255, q, v, p);
            case 2:
                return System.Drawing.Color.FromArgb(255, p, v, t);
            case 3:
                return System.Drawing.Color.FromArgb(255, p, q, v);
            case 4:
                return System.Drawing.Color.FromArgb(255, t, p, v);
            default:
                return System.Drawing.Color.FromArgb(255, v, p, q);
        }
    }
}
