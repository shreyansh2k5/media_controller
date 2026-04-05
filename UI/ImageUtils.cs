// File: UI/ImageUtils.cs
using System.Drawing;

namespace MiniFlyout.UI
{
    public static class ImageUtils
    {
        public static Color GetAmbientGlowColor(Image? image)
        {
            if (image == null) return Color.Black;

            try
            {
                // Resize the image to 1x1 pixel to instantly get the average color mathematically
                using var bitmap = new Bitmap(1, 1);
                using var graphics = Graphics.FromImage(bitmap);
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(image, 0, 0, 1, 1);
                var dominantColor = bitmap.GetPixel(0, 0);

                // Darken the color to act as a subtle ambient glow rather than a blinding solid color.
                // This ensures the white text on the widget remains readable.
                int r = (int)(dominantColor.R * 0.25);
                int g = (int)(dominantColor.G * 0.25);
                int b = (int)(dominantColor.B * 0.25);

                return Color.FromArgb(r, g, b);
            }
            catch
            {
                return Color.Black;
            }
        }
    }
}