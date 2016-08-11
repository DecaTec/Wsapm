using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Wsapm.Wpf.Controls.Extensions
{
    /// <summary>
    /// Extension class for Icon.
    /// </summary>
    public static class IconExtension
    {
        /// <summary>
        /// Gets an ImageSource from Icon.
        /// </summary>
        /// <param name="icon">The Icon to convert.</param>
        /// <returns>The Icon as ImageSource.</returns>
        public static ImageSource ToImageSource(this Icon icon)
        {
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            return imageSource;
        }
    }
}
