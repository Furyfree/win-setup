using System.Runtime.InteropServices;

namespace WinSetup;

public static class Notify
{
    private static readonly IntPtr HwndBroadcast = new(0xffff);

    public static void SettingsChanged()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        SendMessageTimeout(HwndBroadcast, 0x001A, IntPtr.Zero, "ImmersiveColorSet", 0x0002, 1000, out _);
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr SendMessageTimeout(
        IntPtr hWnd, uint message, IntPtr wParam, string lParam, uint flags, uint timeout, out IntPtr result);
}
