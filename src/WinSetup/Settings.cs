using Microsoft.Win32;

namespace WinSetup;

public enum SettingState { Matches, Differs, Missing }

public record Setting(
    string Why,
    string Hive,
    string Key,
    string Name,
    object Value,
    RegistryValueKind Kind,
    int? ByteIndex = null)
{
    public static readonly Setting[] All =
    [
        // Taskbar
        new("Center taskbar icons", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAl", 1, RegistryValueKind.DWord),
        new("Auto-hide the taskbar", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\StuckRects3", "Settings", (byte)0x01, RegistryValueKind.Binary, ByteIndex: 8),
        new("Hide the taskbar search box", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", 0, RegistryValueKind.DWord),
        new("Hide the Task View button", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTaskViewButton", 0, RegistryValueKind.DWord),
        new("Hide the Widgets button", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarDa", 0, RegistryValueKind.DWord),
        new("Hide the Chat button", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarMn", 0, RegistryValueKind.DWord),
        new("Hide the Copilot button", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowCopilotButton", 0, RegistryValueKind.DWord),
        new("Enable End task on taskbar right-click", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarDeveloperSettings", "TaskbarEndTask", 1, RegistryValueKind.DWord),

        // Effects
        new("Disable taskbar animations", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAnimations", 0, RegistryValueKind.DWord),
        new("Disable list view alpha select", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ListviewAlphaSelect", 0, RegistryValueKind.DWord),
        new("Disable list view drop shadows", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ListviewShadow", 0, RegistryValueKind.DWord),
        new("No menu show delay", "HKCU", @"Control Panel\Desktop", "MenuShowDelay", "0", RegistryValueKind.String),
        new("Disable transparency effects", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", 0, RegistryValueKind.DWord),

        // Explorer
        new("Show file extensions in Explorer", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 0, RegistryValueKind.DWord),
        new("Show hidden files in Explorer", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Hidden", 1, RegistryValueKind.DWord),
        new("Open Explorer to This PC", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "LaunchTo", 1, RegistryValueKind.DWord),
        new("Hide recent files in Quick Access", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowRecent", 0, RegistryValueKind.DWord),
        new("Hide frequent folders in Quick Access", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowFrequent", 0, RegistryValueKind.DWord),
        new("Disable sync provider notifications", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowSyncProviderNotifications", 0, RegistryValueKind.DWord),

        // Start menu and suggestions
        new("Hide the Recommended section in Start", "HKLM", @"SOFTWARE\Policies\Microsoft\Windows\Explorer", "HideRecommendedSection", 1, RegistryValueKind.DWord),
        new("Hide the Recommended section in Start (policy manager)", "HKLM", @"SOFTWARE\Microsoft\PolicyManager\current\device\Start", "HideRecommendedSection", 1, RegistryValueKind.DWord),
        new("Hide the Recommended section in Start (user policy)", "HKCU", @"Software\Policies\Microsoft\Windows\Explorer", "HideRecommendedSection", 1, RegistryValueKind.DWord),
        new("Disable Start menu recommendations", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_IrisRecommendations", 0, RegistryValueKind.DWord),
        new("Disable web results in Start search", "HKCU", @"Software\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", 1, RegistryValueKind.DWord),
        new("Disable Bing in Start search (25H2)", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Search", "BingSearchEnabled", 0, RegistryValueKind.DWord),
        new("Block automatically installed suggested apps", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SilentInstalledAppsEnabled", 0, RegistryValueKind.DWord),
        new("Disable Start menu subscribed suggestions", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338388Enabled", 0, RegistryValueKind.DWord),
        new("Disable Settings app suggestions", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SystemPaneSuggestionsEnabled", 0, RegistryValueKind.DWord),
        new("Disable advertising ID", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 0, RegistryValueKind.DWord),

        // Gaming
        new("Disable Game DVR background recording", "HKCU", @"System\GameConfigStore", "GameDVR_Enabled", 0, RegistryValueKind.DWord),
        new("Disable app capture", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", 0, RegistryValueKind.DWord),
        new("Enable Game Mode", "HKCU", @"Software\Microsoft\GameBar", "AutoGameModeEnabled", 1, RegistryValueKind.DWord),

        // Mouse
        new("Disable mouse acceleration speed", "HKCU", @"Control Panel\Mouse", "MouseSpeed", "0", RegistryValueKind.String),
        new("Disable mouse acceleration threshold 1", "HKCU", @"Control Panel\Mouse", "MouseThreshold1", "0", RegistryValueKind.String),
        new("Disable mouse acceleration threshold 2", "HKCU", @"Control Panel\Mouse", "MouseThreshold2", "0", RegistryValueKind.String),
    ];

    public SettingState Check()
    {
        if (!OperatingSystem.IsWindows())
        {
            return SettingState.Missing;
        }

        var current = Read();
        return current is null ? SettingState.Missing : Matches(current) ? SettingState.Matches : SettingState.Differs;
    }

    public bool Matches(object? current)
    {
        if (current is null)
        {
            return false;
        }

        return Kind switch
        {
            RegistryValueKind.DWord or RegistryValueKind.QWord => Convert.ToInt64(current) == Convert.ToInt64(Value),
            RegistryValueKind.Binary => current is byte[] bytes
                && ByteIndex is int index
                && index < bytes.Length
                && (bytes[index] & Convert.ToByte(Value)) == Convert.ToByte(Value),
            _ => string.Equals(Convert.ToString(current), Convert.ToString(Value), StringComparison.Ordinal),
        };
    }

    public void Write()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        using var baseKey = RegistryKey.OpenBaseKey(HiveOf(), RegistryView.Default);
        using var key = baseKey.CreateSubKey(Key, writable: true);
        if (Kind == RegistryValueKind.Binary && ByteIndex is int index)
        {
            var bytes = key.GetValue(Name) as byte[]
                ?? throw new InvalidOperationException($"{Name} has no binary value to modify");
            if (index >= bytes.Length)
            {
                throw new InvalidOperationException($"{Name} is only {bytes.Length} bytes; index {index} is out of range");
            }

            var updated = (byte[])bytes.Clone();
            updated[index] |= Convert.ToByte(Value);
            key.SetValue(Name, updated, RegistryValueKind.Binary);
            return;
        }

        key.SetValue(Name, Value, Kind);
    }

    public object? Read()
    {
        if (!OperatingSystem.IsWindows())
        {
            return null;
        }

        using var baseKey = RegistryKey.OpenBaseKey(HiveOf(), RegistryView.Default);
        using var key = baseKey.OpenSubKey(Key);
        return key?.GetValue(Name);
    }

    private RegistryHive HiveOf() => Hive switch
    {
        "HKCU" => RegistryHive.CurrentUser,
        "HKLM" => RegistryHive.LocalMachine,
        _ => throw new InvalidOperationException($"unknown hive {Hive}"),
    };
}
