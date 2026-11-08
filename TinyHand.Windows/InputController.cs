using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace TinyHand.Windows;

internal sealed class InputController : IDisposable
{
    private readonly OverlayWindow overlay;
    private readonly DispatcherTimer timer;
    private readonly NativeMethods.HookProc mouseProc;
    private readonly NativeMethods.HookProc keyboardProc;
    private readonly nint mouseHook;
    private readonly nint keyboardHook;
    private bool disposed;
    private bool capturedLeftButton;

    public InputController(OverlayWindow overlay)
    {
        this.overlay = overlay;
        mouseProc = MouseHook;
        keyboardProc = KeyboardHook;
        mouseHook = NativeMethods.SetWindowsHookEx(NativeMethods.WhMouseLl, mouseProc, 0, 0);
        keyboardHook = NativeMethods.SetWindowsHookEx(NativeMethods.WhKeyboardLl, keyboardProc, 0, 0);
        if (mouseHook == 0 || keyboardHook == 0)
        {
            if (mouseHook != 0) NativeMethods.UnhookWindowsHookEx(mouseHook);
            if (keyboardHook != 0) NativeMethods.UnhookWindowsHookEx(keyboardHook);
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not install TinyHand's input hooks.");
        }

        timer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        timer.Tick += (_, _) =>
        {
            if (overlay.PointerActive)
            {
                overlay.UpdatePose();
                overlay.AddDrawingPoint();
            }
        };
        timer.Start();
    }

    public void Toggle() => overlay.SetEnabled(!overlay.PointerActive);

    public void Dispose()
    {
        if (disposed)
            return;
        disposed = true;
        timer.Stop();
        NativeMethods.UnhookWindowsHookEx(mouseHook);
        NativeMethods.UnhookWindowsHookEx(keyboardHook);
    }

    private nint MouseHook(int code, nint wParam, nint lParam)
    {
        if (code >= 0)
        {
            var message = wParam.ToInt32();
            if (message == NativeMethods.WmLButtonDown && overlay.PointerActive && !IsForegroundApplication())
            {
                capturedLeftButton = true;
                overlay.Dispatcher.BeginInvoke(() => overlay.SetHeld(true));
                return 1;
            }
            if (message == NativeMethods.WmLButtonUp && capturedLeftButton)
            {
                capturedLeftButton = false;
                overlay.Dispatcher.BeginInvoke(() => overlay.SetHeld(false));
                return 1;
            }
        }
        return NativeMethods.CallNextHookEx(mouseHook, code, wParam, lParam);
    }

    private static bool IsForegroundApplication()
    {
        var foreground = NativeMethods.GetForegroundWindow();
        return foreground != 0 &&
               NativeMethods.GetWindowThreadProcessId(foreground, out var processId) != 0 &&
               processId == Environment.ProcessId;
    }

    private nint KeyboardHook(int code, nint wParam, nint lParam)
    {
        if (code >= 0 && (wParam.ToInt32() is NativeMethods.WmKeyDown or NativeMethods.WmSysKeyDown))
        {
            var key = Marshal.ReadInt32(lParam);
            var control = (NativeMethods.GetAsyncKeyState(NativeMethods.VkControl) & 0x8000) != 0;
            var alt = (NativeMethods.GetAsyncKeyState(NativeMethods.VkMenu) & 0x8000) != 0;

            if (overlay.PointerActive && key == NativeMethods.VkEscape)
            {
                overlay.Dispatcher.BeginInvoke(() => overlay.SetEnabled(false));
                return 1;
            }
            if (control && alt && key == NativeMethods.VkH)
            {
                overlay.Dispatcher.BeginInvoke(Toggle);
                return 1;
            }
            if (control && alt && key == NativeMethods.VkX)
            {
                overlay.Dispatcher.BeginInvoke(overlay.ClearDrawing);
                return 1;
            }
        }
        return NativeMethods.CallNextHookEx(keyboardHook, code, wParam, lParam);
    }
}
