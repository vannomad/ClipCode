using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hardcodet.Wpf.TaskbarNotification;
using ZXing;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Point = System.Drawing.Point;

namespace ClipCode
{
  /// <summary>
  ///   Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
      Visibility = Visibility.Hidden;
    }

    public static Bitmap? BitmapSourceToBitmap(BitmapSource? source)
    {
      if (source is null)
        return null;

      var pixelFormat = PixelFormat.Format32bppArgb; //Bgr32 equiv default
      if (source.Format == PixelFormats.Bgr24) pixelFormat = PixelFormat.Format24bppRgb;
      else if (source.Format == PixelFormats.Pbgra32) pixelFormat = PixelFormat.Format32bppPArgb;
      else if (source.Format == PixelFormats.Prgba64) pixelFormat = PixelFormat.Format64bppPArgb;

      var bmp = new Bitmap(
        source.PixelWidth,
        source.PixelHeight,
        pixelFormat);

      var data = bmp.LockBits(
        new Rectangle(new Point(), bmp.Size),
        ImageLockMode.WriteOnly,
        pixelFormat);

      source.CopyPixels(
        Int32Rect.Empty,
        data.Scan0,
        data.Height * data.Stride,
        data.Stride);

      bmp.UnlockBits(data);

      return bmp;
    }

    private void ScanClipboard()
    {
      if (!Clipboard.ContainsImage())
      {
        TrayIcon.ShowBalloonTip("", "No image in Clipboard", BalloonIcon.Info);
        return;
      }

      var clipboardBitmapSource = Clipboard.GetImage();

      var clipboardBitmap = BitmapSourceToBitmap(clipboardBitmapSource);

      var barcodeResult = new BarcodeReader().Decode(clipboardBitmap);

      var barcodeResultText = barcodeResult?.Text;
      if (string.IsNullOrEmpty(barcodeResultText))
      {
        TrayIcon.ShowBalloonTip("", "Couldn't recognize Barcode", BalloonIcon.Error);
        return;
      }

      TrayIcon.ShowBalloonTip("Success", "Value: '" + barcodeResultText + "' has been copied.", BalloonIcon.None);
      Clipboard.SetText(barcodeResultText);
    }

    private void TrayIcon_OnTrayMouseDoubleClick(object sender, RoutedEventArgs e)
    {
      ScanClipboard();
    }

    private void MenuScan_OnClick(object sender, RoutedEventArgs e)
    {
      ScanClipboard();
    }

    private void MenuQuit_OnClick(object sender, RoutedEventArgs e)
    {
      Close();
    }
  }
}