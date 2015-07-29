using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Glow.Controls
{
    public class OnColorChangedArgs
    {
        public byte Red;
        public byte Blue;
        public byte Green;
    }

    public sealed partial class ColorPicker : UserControl
    {
        /// <summary>
        /// Fired when the color changes
        /// </summary>
        public event EventHandler<OnColorChangedArgs> OnColorChanged;

        #region Source Image Logic

        /// <summary>
        /// This it how we get the post form the xmal binding.
        /// </summary>
        public string Source
        {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(
                "MarkDown",                     // The name of the DependencyProperty
                typeof(string),                 // The type of the DependencyProperty
                typeof(ColorPicker),            // The type of the owner of the DependencyProperty
                new PropertyMetadata(           // OnBlinkChanged will be called when Blink changes
                    false,                      // The default value of the DependencyProperty
                    new PropertyChangedCallback(OnSourceChangedStatic)
                ));

        private static void OnSourceChangedStatic(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var instance = d as ColorPicker;
            if (instance != null)
            {
                // Send the post to the class.
                instance.OnSourceChanged(e.NewValue.GetType() == typeof(string) ? (string)e.NewValue : "");
            }
        }

        #endregion

        // Private vars
        int m_imageHeight;
        int m_imageWidth;
        byte[] m_imagePixelArray = null;

        public ColorPicker()
        {
            this.InitializeComponent();

            // Turn on manipulation
            ui_colorImage.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
        }

        private void ColorImage_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            FireColorChanged(e.Position.X, e.Position.Y);
        }

        private void ui_colorImage_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint point = e.GetCurrentPoint(ui_colorImage);
            FireColorChanged(point.Position.X, point.Position.Y);
        }

        private void OnSourceChanged(string newSource)
        {
            if(String.IsNullOrWhiteSpace(newSource))
            {
                return;
            }

            // Set the image
            ui_colorImage.Source = new BitmapImage(new Uri(newSource, UriKind.Absolute));

            // Load the image into memory
            int lastSlash = newSource.LastIndexOf("/") + 1;
            OpenImageFile(newSource.Substring(lastSlash));
        }

        private void FireColorChanged(double screenImageX, double screenImageY)
        {
            double x = screenImageX / ui_colorImage.ActualWidth;
            double y = screenImageY / ui_colorImage.ActualHeight;
            byte red, green, blue;
            GetRBGValueForPosition(x, y, out red, out green, out blue);

            // Fire the event.
            if (OnColorChanged != null)
            {
                OnColorChanged(this, new OnColorChangedArgs { Red = red, Green = green, Blue = blue });
            }
        }

        private void GetRBGValueForPosition(double x, double y, out byte red, out byte green, out byte blue)
        {
            red = 0;
            green = 0;
            blue = 0;

            // Check we are good, if not bail
            if (m_imagePixelArray == null || m_imageHeight <= 0 || m_imageWidth <= 0)
            {
                return;
            }

            // Figure out our image x and y
            int imageX = (int)(m_imageWidth * x);
            int imageY = (int)(m_imageHeight * y);

            // Get the starting post, each pixel is 4 bytes due to alpha
            int arrayStartingPos = (imageY * m_imageWidth * 4) + (imageX * 4);

            // Check for errors
            if(arrayStartingPos + 3 > m_imagePixelArray.Length || arrayStartingPos < 0) 
            {
                return;
            }

            blue = m_imagePixelArray[arrayStartingPos];
            green = m_imagePixelArray[arrayStartingPos + 1];
            red = m_imagePixelArray[arrayStartingPos + 2];
        }

        private async void OpenImageFile(string fileName)
        {
            StorageFolder installFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFolder subFolder = await installFolder.GetFolderAsync("Assets");
            subFolder = await subFolder.GetFolderAsync("Pickers");
            StorageFile file = await subFolder.GetFileAsync(fileName);

            using (IRandomAccessStreamWithContentType reader = await file.OpenReadAsync())
            {
                // There has to be a better way to do this.
                BitmapImage image = new BitmapImage();
                await image.SetSourceAsync(reader);
                WriteableBitmap imageBitmap = new WriteableBitmap(image.PixelWidth, image.PixelHeight);
                reader.Seek(0);
                await imageBitmap.SetSourceAsync(reader);
                m_imagePixelArray = imageBitmap.PixelBuffer.ToArray();
                m_imageHeight = imageBitmap.PixelHeight;
                m_imageWidth = imageBitmap.PixelWidth;
            }
        }
    }
}
