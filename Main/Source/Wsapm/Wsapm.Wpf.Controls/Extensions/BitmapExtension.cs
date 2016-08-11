using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace Wsapm.Wpf.Controls.Extensions
{
    /// <summary>
    /// Bitmap extension
    /// </summary>
    public static class BitmapExtension
    {
        /// <summary>
        /// Gets an BitmapImage from a Bitmap.
        /// </summary>
        /// <param name="bitmap">The Bitmap to convert.</param>
        /// <param name="imageFormat">The ImageFormat of the Bitmap.</param>
        /// <returns>The Bitmap as BitmapImage.</returns>
        public static BitmapImage ToImageSource(this Bitmap bitmap, ImageFormat imageFormat)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, imageFormat);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }
    }
}
