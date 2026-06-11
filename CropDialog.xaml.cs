using GameLauncher.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace GameLauncher;

public sealed partial class CropDialog : ContentDialog
{
    private readonly string _imagePath;
    private bool _isDragging;
    private bool _isResizing;
    private string _activeHandle = "";
    private Point _dragStart;
    private double _cropLeft, _cropTop, _cropWidth, _cropHeight;
    private double _imgLeft, _imgTop, _imgWidth, _imgHeight;
    private double _aspectRatio = 0;

    public string? CroppedImagePath { get; private set; }

    public CropDialog(string imagePath)
    {
        _imagePath = imagePath;
        InitializeComponent();
        Loaded += CropDialog_Loaded;
    }

    private async void CropDialog_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var file = await StorageFile.GetFileFromPathAsync(_imagePath);
            var stream = await file.OpenAsync(FileAccessMode.Read);
            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(stream);
            SourceImage.Source = bitmap;

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                InitCropArea();
            });
        }
        catch
        {
        }
    }

    private void InitCropArea()
    {
        if (SourceImage.ActualWidth <= 0 || SourceImage.ActualHeight <= 0) return;

        _imgWidth = SourceImage.ActualWidth;
        _imgHeight = SourceImage.ActualHeight;
        _imgLeft = (CropCanvas.ActualWidth - _imgWidth) / 2;
        _imgTop = (CropCanvas.ActualHeight - _imgHeight) / 2;

        var margin = 40.0;
        _cropLeft = _imgLeft + margin;
        _cropTop = _imgTop + margin;
        _cropWidth = _imgWidth - margin * 2;
        _cropHeight = _imgHeight - margin * 2;

        if (_aspectRatio > 0)
        {
            var currentRatio = _cropWidth / _cropHeight;
            if (currentRatio > _aspectRatio)
                _cropWidth = _cropHeight * _aspectRatio;
            else
                _cropHeight = _cropWidth / _aspectRatio;

            _cropLeft = _imgLeft + (_imgWidth - _cropWidth) / 2;
            _cropTop = _imgTop + (_imgHeight - _cropHeight) / 2;
        }

        UpdateCropOverlay();
        ShowCropUI();
    }

    private void ShowCropUI()
    {
        CropOverlay.Visibility = Visibility.Visible;
        HandleTopLeft.Visibility = Visibility.Visible;
        HandleTopRight.Visibility = Visibility.Visible;
        HandleBottomLeft.Visibility = Visibility.Visible;
        HandleBottomRight.Visibility = Visibility.Visible;
    }

    private void UpdateCropOverlay()
    {
        Canvas.SetLeft(CropOverlay, _cropLeft);
        Canvas.SetTop(CropOverlay, _cropTop);
        CropOverlay.Width = _cropWidth;
        CropOverlay.Height = _cropHeight;

        Canvas.SetLeft(HandleTopLeft, _cropLeft - 6);
        Canvas.SetTop(HandleTopLeft, _cropTop - 6);
        Canvas.SetLeft(HandleTopRight, _cropLeft + _cropWidth - 6);
        Canvas.SetTop(HandleTopRight, _cropTop - 6);
        Canvas.SetLeft(HandleBottomLeft, _cropLeft - 6);
        Canvas.SetTop(HandleBottomLeft, _cropTop + _cropHeight - 6);
        Canvas.SetLeft(HandleBottomRight, _cropLeft + _cropWidth - 6);
        Canvas.SetTop(HandleBottomRight, _cropTop + _cropHeight - 6);

        DrawDimOverlay();
    }

    private void DrawDimOverlay()
    {
        DimOverlay.Children.Clear();

        var darkBrush = new SolidColorBrush(Microsoft.UI.Colors.Black) { Opacity = 0.5 };

        AddDimRect(darkBrush, 0, 0, CropCanvas.ActualWidth, _cropTop);
        AddDimRect(darkBrush, 0, _cropTop, _cropLeft, _cropHeight);
        AddDimRect(darkBrush, _cropLeft + _cropWidth, _cropTop, CropCanvas.ActualWidth - _cropLeft - _cropWidth, _cropHeight);
        AddDimRect(darkBrush, 0, _cropTop + _cropHeight, CropCanvas.ActualWidth, CropCanvas.ActualHeight - _cropTop - _cropHeight);
    }

    private void AddDimRect(Brush brush, double left, double top, double width, double height)
    {
        if (width <= 0 || height <= 0) return;
        var rect = new Microsoft.UI.Xaml.Shapes.Rectangle
        {
            Width = width,
            Height = height,
            Fill = brush
        };
        Canvas.SetLeft(rect, left);
        Canvas.SetTop(rect, top);
        DimOverlay.Children.Add(rect);
    }

    private void CropCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (CropOverlay.Visibility != Visibility.Visible) return;

        var pos = e.GetCurrentPoint(CropCanvas);
        _dragStart = new Point(pos.Position.X, pos.Position.Y);

        if (IsInsideCrop(pos.Position.X, pos.Position.Y))
        {
            _isDragging = true;
            (sender as UIElement)?.CapturePointer(e.Pointer);
        }
    }

    private void CropCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!_isDragging) return;

        var pos = e.GetCurrentPoint(CropCanvas);
        var dx = pos.Position.X - _dragStart.X;
        var dy = pos.Position.Y - _dragStart.Y;

        var newLeft = _cropLeft + dx;
        var newTop = _cropTop + dy;

        newLeft = Math.Max(_imgLeft, Math.Min(newLeft, _imgLeft + _imgWidth - _cropWidth));
        newTop = Math.Max(_imgTop, Math.Min(newTop, _imgTop + _imgHeight - _cropHeight));

        _cropLeft = newLeft;
        _cropTop = newTop;
        _dragStart = new Point(pos.Position.X, pos.Position.Y);

        UpdateCropOverlay();
    }

    private void CropCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        _isDragging = false;
        _isResizing = false;
        _activeHandle = "";
        (sender as UIElement)?.ReleasePointerCaptures();
    }

    private void Handle_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (ReferenceEquals(sender, HandleTopLeft)) _activeHandle = "tl";
        else if (ReferenceEquals(sender, HandleTopRight)) _activeHandle = "tr";
        else if (ReferenceEquals(sender, HandleBottomLeft)) _activeHandle = "bl";
        else if (ReferenceEquals(sender, HandleBottomRight)) _activeHandle = "br";

        _isResizing = true;
        var pos = e.GetCurrentPoint(CropCanvas);
        _dragStart = new Point(pos.Position.X, pos.Position.Y);
        (sender as UIElement)?.CapturePointer(e.Pointer);
        e.Handled = true;
    }

    private void Handle_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
    {
        if (!_isResizing) return;

        var dx = e.Delta.Translation.X;
        var dy = e.Delta.Translation.Y;

        var minSize = 30.0;

        switch (_activeHandle)
        {
            case "tl":
                {
                    var newLeft = Math.Max(_imgLeft, _cropLeft + dx);
                    var newTop = Math.Max(_imgTop, _cropTop + dy);
                    var maxW = _cropLeft + _cropWidth - newLeft;
                    var maxH = _cropTop + _cropHeight - newTop;
                    if (maxW >= minSize && maxH >= minSize)
                    {
                        if (_aspectRatio > 0)
                        {
                            var ratioW = maxH * _aspectRatio;
                            if (ratioW > maxW) { newTop = _cropTop + _cropHeight - maxW / _aspectRatio; }
                            else { newLeft = _cropLeft + _cropWidth - maxH * _aspectRatio; }
                        }
                        _cropLeft = newLeft;
                        _cropTop = newTop;
                        _cropWidth = _cropLeft + _cropWidth - newLeft;
                        _cropHeight = _cropTop + _cropHeight - newTop;
                    }
                }
                break;
            case "tr":
                {
                    var newW = Math.Min(_imgLeft + _imgWidth - _cropLeft, _cropWidth + dx);
                    var newTop = Math.Max(_imgTop, _cropTop + dy);
                    var maxH = _cropTop + _cropHeight - newTop;
                    if (newW >= minSize && maxH >= minSize)
                    {
                        if (_aspectRatio > 0)
                        {
                            var ratioW = maxH * _aspectRatio;
                            if (ratioW > newW) { newTop = _cropTop + _cropHeight - newW / _aspectRatio; }
                            else { newW = maxH * _aspectRatio; }
                        }
                        _cropWidth = newW;
                        _cropTop = newTop;
                        _cropHeight = _cropTop + _cropHeight - newTop;
                    }
                }
                break;
            case "bl":
                {
                    var newLeft = Math.Max(_imgLeft, _cropLeft + dx);
                    var newH = Math.Min(_imgTop + _imgHeight - _cropTop, _cropHeight + dy);
                    var maxW = _cropLeft + _cropWidth - newLeft;
                    if (maxW >= minSize && newH >= minSize)
                    {
                        if (_aspectRatio > 0)
                        {
                            var ratioH = maxW / _aspectRatio;
                            if (ratioH > newH) { newLeft = _cropLeft + _cropWidth - newH * _aspectRatio; }
                            else { newH = maxW / _aspectRatio; }
                        }
                        _cropLeft = newLeft;
                        _cropHeight = newH;
                        _cropWidth = _cropLeft + _cropWidth - newLeft;
                    }
                }
                break;
            case "br":
                {
                    var newW = Math.Min(_imgLeft + _imgWidth - _cropLeft, _cropWidth + dx);
                    var newH = Math.Min(_imgTop + _imgHeight - _cropTop, _cropHeight + dy);
                    if (newW >= minSize && newH >= minSize)
                    {
                        if (_aspectRatio > 0)
                        {
                            var ratioH = newW / _aspectRatio;
                            if (ratioH > newH) { newW = newH * _aspectRatio; }
                            else { newH = newW / _aspectRatio; }
                        }
                        _cropWidth = newW;
                        _cropHeight = newH;
                    }
                }
                break;
        }

        UpdateCropOverlay();
    }

    private bool IsInsideCrop(double x, double y)
    {
        return x >= _cropLeft && x <= _cropLeft + _cropWidth &&
               y >= _cropTop && y <= _cropTop + _cropHeight;
    }

    private void AspectBtn_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string tag)
        {
            switch (tag)
            {
                case "16:9": _aspectRatio = 16.0 / 9.0; break;
                case "4:3": _aspectRatio = 4.0 / 3.0; break;
                case "1:1": _aspectRatio = 1.0; break;
                case "free": _aspectRatio = 0; break;
            }
            InitCropArea();
        }
    }

    private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var deferral = args.GetDeferral();

        try
        {
            var file = await StorageFile.GetFileFromPathAsync(_imagePath);
            var stream = await file.OpenAsync(FileAccessMode.Read);
            var decoder = await BitmapDecoder.CreateAsync(stream);

            var scaleX = decoder.PixelWidth / _imgWidth;
            var scaleY = decoder.PixelHeight / _imgHeight;

            var cropX = (uint)((_cropLeft - _imgLeft) * scaleX);
            var cropY = (uint)((_cropTop - _imgTop) * scaleY);
            var cropW = (uint)(_cropWidth * scaleX);
            var cropH = (uint)(_cropHeight * scaleY);

            cropX = Math.Min(cropX, decoder.PixelWidth - 1);
            cropY = Math.Min(cropY, decoder.PixelHeight - 1);
            cropW = Math.Min(cropW, decoder.PixelWidth - cropX);
            cropH = Math.Min(cropH, decoder.PixelHeight - cropY);

            var transform = new BitmapTransform
            {
                Bounds = new BitmapBounds
                {
                    X = cropX,
                    Y = cropY,
                    Width = cropW,
                    Height = cropH
                }
            };

            var pixelData = await decoder.GetPixelDataAsync(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Premultiplied,
                transform,
                ExifOrientationMode.IgnoreExifOrientation,
                ColorManagementMode.DoNotColorManage);

            var croppedBitmap = new WriteableBitmap((int)cropW, (int)cropH);
            var pixels = pixelData.DetachPixelData();
            using (var croppedStream = new Windows.Storage.Streams.InMemoryRandomAccessStream())
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, croppedStream);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, cropW, cropH, 96, 96, pixels);
                await encoder.FlushAsync();

                var appDataPath = ImageService.GetImageDirectory();

                var fileName = $"crop_{DateTime.Now:yyyyMMddHHmmss}.png";
                var outputPath = Path.Combine(appDataPath, fileName);

                croppedStream.Seek(0);
                using (var fileStream = File.Create(outputPath))
                {
                    using (var stream2 = croppedStream.AsStreamForRead())
                    {
                        await stream2.CopyToAsync(fileStream);
                    }
                }

                CroppedImagePath = outputPath;
            }
        }
        catch (Exception ex)
        {
            var errorDialog = new ContentDialog
            {
                Title = "裁切失败",
                Content = ex.Message,
                CloseButtonText = "确定",
                XamlRoot = this.XamlRoot
            };
            await errorDialog.ShowAsync();
            args.Cancel = true;
        }

        deferral.Complete();
    }
}
