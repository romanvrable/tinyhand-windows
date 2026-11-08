using System.Windows;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfPoint = System.Windows.Point;

namespace TinyHand.Windows;

public partial class OverlayWindow : Window
{
    private bool isDrawing;
    private Polyline? stroke;
    private WpfPoint? lastPoint;
    private volatile bool enabled;

    public bool PointerActive => enabled;
    public event Action? EnabledChanged;

    public OverlayWindow()
    {
        InitializeComponent();
        Left = SystemParameters.VirtualScreenLeft;
        Top = SystemParameters.VirtualScreenTop;
        Width = SystemParameters.VirtualScreenWidth;
        Height = SystemParameters.VirtualScreenHeight;
        SourceInitialized += (_, _) =>
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            var style = NativeMethods.GetWindowLongPtr(hwnd, NativeMethods.GwlExStyle).ToInt64();
            NativeMethods.SetWindowLongPtr(hwnd, NativeMethods.GwlExStyle,
                (nint)(style | NativeMethods.ExTransparent | NativeMethods.ExToolWindow | NativeMethods.ExNoActivate));
        };
        IsVisibleChanged += (_, _) => UpdatePose();
    }

    public void SetEnabled(bool value)
    {
        if (enabled == value)
            return;
        enabled = value;
        if (value)
        {
            Show();
            Topmost = true;
            UpdatePose();
        }
        else
        {
            SetHeld(false);
            Hide();
        }
        EnabledChanged?.Invoke();
    }

    public void SetHand(string name)
    {
        var laser = name == "laser";
        HandImage.Visibility = laser ? Visibility.Collapsed : Visibility.Visible;
        LaserDot.Visibility = laser ? Visibility.Visible : Visibility.Collapsed;
        if (laser)
            return;
        HandImage.Source = new BitmapImage(new Uri(
            $"pack://application:,,,/TinyHand;component/assets/hand_{name}.png"));
    }

    public void UpdatePose()
    {
        if (!enabled || !NativeMethods.GetCursorPos(out var cursor))
            return;

        var point = PointFromScreen(new WpfPoint(cursor.X, cursor.Y));
        if (LaserDot.Visibility == Visibility.Visible)
        {
            Canvas.SetLeft(LaserDot, point.X - LaserDot.Width / 2);
            Canvas.SetTop(LaserDot, point.Y - LaserDot.Height / 2);
        }
        else
        {
            Canvas.SetLeft(HandImage, point.X - HandImage.Width / 2);
            Canvas.SetTop(HandImage, point.Y - 8);
        }
    }

    public void SetHeld(bool value)
    {
        if (isDrawing == value)
            return;

        isDrawing = value;
        if (value)
        {
            stroke = new Polyline
            {
                Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(135, 84, 242)),
                StrokeThickness = 5,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round,
                IsHitTestVisible = false
            };
            DrawingSurface.Children.Add(stroke);
            lastPoint = null;
        }
        else
        {
            stroke = null;
            lastPoint = null;
        }
    }

    public void AddDrawingPoint()
    {
        if (!isDrawing || stroke is null || !NativeMethods.GetCursorPos(out var cursor))
            return;

        var point = PointFromScreen(new WpfPoint(cursor.X, cursor.Y));
        if (lastPoint is null || (point - lastPoint.Value).Length >= 2)
        {
            stroke.Points.Add(point);
            lastPoint = point;
        }
    }

    public void ClearDrawing()
    {
        SetHeld(false);
        DrawingSurface.Children.Clear();
        DrawingSurface.Children.Add(HandImage);
        DrawingSurface.Children.Add(LaserDot);
    }
}
