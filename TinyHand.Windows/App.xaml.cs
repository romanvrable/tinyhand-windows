using System.Windows;

namespace TinyHand.Windows;

public partial class App : System.Windows.Application
{
    private TrayController? tray;
    private OverlayWindow? overlay;
    private InputController? input;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        overlay = new OverlayWindow();
        input = new InputController(overlay);
        tray = new TrayController(overlay, input);
        tray.ShowSettings();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        input?.Dispose();
        tray?.Dispose();
        overlay?.Close();
        base.OnExit(e);
    }
}
