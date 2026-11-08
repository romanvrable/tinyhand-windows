using System.Windows;
using System.Windows.Forms;

namespace TinyHand.Windows;

internal sealed class TrayController : IDisposable
{
    private readonly OverlayWindow overlay;
    private readonly InputController input;
    private readonly NotifyIcon icon;
    private SettingsWindow? settings;

    public TrayController(OverlayWindow overlay, InputController input)
    {
        this.overlay = overlay;
        this.input = input;
        var menu = new ContextMenuStrip();
        menu.Items.Add("Turn pointer on / off  (Ctrl+Alt+H)", null, (_, _) => input.Toggle());
        menu.Items.Add("Clear drawing  (Ctrl+Alt+X)", null, (_, _) => overlay.ClearDrawing());
        menu.Items.Add("Settings", null, (_, _) => ShowSettings());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => System.Windows.Application.Current.Shutdown());
        icon = new NotifyIcon
        {
            Text = "TinyHand",
            Icon = System.Drawing.SystemIcons.Hand,
            ContextMenuStrip = menu,
            Visible = true
        };
        icon.DoubleClick += (_, _) => input.Toggle();
    }

    public void ShowSettings()
    {
        if (settings is null)
        {
            settings = new SettingsWindow(overlay, input);
            settings.Closed += (_, _) => settings = null;
        }
        settings.Show();
        settings.Topmost = true;
        settings.Activate();
    }

    public void Dispose()
    {
        icon.Visible = false;
        icon.Dispose();
    }
}
