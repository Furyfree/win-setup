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
    int? ByteIndex = null,
    bool IgnoreTimestamps = false)
{
    public static readonly string WallpaperPath = Path.Combine(
        Paths.UserProfile, "Pictures", "Wallpapers", "charcoal-amber-mountain-horizon.jpg");

    public static readonly Setting[] All =
    [
        // Taskbar
        new("Center taskbar icons", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAl", 1, RegistryValueKind.DWord),
        new("Hide the taskbar search box", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", 0, RegistryValueKind.DWord),
        new("Hide the Task View button", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTaskViewButton", 0, RegistryValueKind.DWord),
        new("Enable End task on taskbar right-click", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarDeveloperSettings", "TaskbarEndTask", 1, RegistryValueKind.DWord),
        new("Hide the Widgets feed", "HKLM", @"SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests", 0, RegistryValueKind.DWord),
        new("Hide the taskbar Copilot companion", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarCompanion", 0, RegistryValueKind.DWord),
        new("Hide the Copilot PWA pin", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "CopilotPWAPin", 0, RegistryValueKind.DWord),
        new("Hide the Recall pin", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "RecallPin", 0, RegistryValueKind.DWord),

        // Effects
        new("Use custom visual effects so the animation switches stick", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects", "VisualFXSetting", 3, RegistryValueKind.DWord),
        new("Disable taskbar animations", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAnimations", 0, RegistryValueKind.DWord),
        new("No menu show delay", "HKCU", @"Control Panel\Desktop", "MenuShowDelay", "0", RegistryValueKind.String),
        new("Enable transparency effects", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", 1, RegistryValueKind.DWord),

        // Wallpaper and lock screen
        new("Set the desktop wallpaper", "HKCU", @"Control Panel\Desktop", "WallPaper", WallpaperPath, RegistryValueKind.String),
        new("Fill the desktop wallpaper", "HKCU", @"Control Panel\Desktop", "WallpaperStyle", "10", RegistryValueKind.String),
        new("Disable wallpaper tiling", "HKCU", @"Control Panel\Desktop", "TileWallpaper", "0", RegistryValueKind.String),
        new("Set the lock screen image", "HKLM", @"SOFTWARE\Microsoft\Windows\CurrentVersion\PersonalizationCSP", "LockScreenImagePath", WallpaperPath, RegistryValueKind.String),
        new("Set the lock screen image URL", "HKLM", @"SOFTWARE\Microsoft\Windows\CurrentVersion\PersonalizationCSP", "LockScreenImageUrl", WallpaperPath, RegistryValueKind.String),
        new("Enable the lock screen image policy", "HKLM", @"SOFTWARE\Microsoft\Windows\CurrentVersion\PersonalizationCSP", "LockScreenImageStatus", 1, RegistryValueKind.DWord),

        // Snapping
        new("Keep window snapping available", "HKCU", @"Control Panel\Desktop", "WindowArrangementActive", "1", RegistryValueKind.String),
        new("Disable snap assist suggestions", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "SnapAssist", 0, RegistryValueKind.DWord),
        new("Disable snap layouts on maximize hover", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "EnableSnapAssistFlyout", 0, RegistryValueKind.DWord),
        new("Disable snap layouts on drag to top", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "EnableSnapBar", 0, RegistryValueKind.DWord),

        // Explorer
        new("Show file extensions in Explorer", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 0, RegistryValueKind.DWord),
        new("Show hidden files in Explorer", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Hidden", 1, RegistryValueKind.DWord),
        new("Open Explorer to This PC", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "LaunchTo", 1, RegistryValueKind.DWord),
        new("Hide recent files in Quick Access", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowRecent", 0, RegistryValueKind.DWord),
        new("Hide frequent folders in Quick Access", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowFrequent", 0, RegistryValueKind.DWord),
        new("Disable Explorer recommendations", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowRecommendations", 0, RegistryValueKind.DWord),
        new("Remove cloud files from Quick Access", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer", "ShowCloudFilesInQuickAccess", 0, RegistryValueKind.DWord),
        new("Disable sync provider notifications", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowSyncProviderNotifications", 0, RegistryValueKind.DWord),
        new("Disable most-used app tracking", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackProgs", 0, RegistryValueKind.DWord),
        new("Remove Home from Explorer", "HKCU", @"Software\Classes\CLSID\{f874310e-b6b7-47dc-bc84-b9e6b38f5903}", "System.IsPinnedToNameSpaceTree", 0, RegistryValueKind.DWord),
        new("Remove Gallery from Explorer", "HKCU", @"Software\Classes\CLSID\{e88865ea-0e1c-4e20-9aa6-edcd0212c87c}", "System.IsPinnedToNameSpaceTree", 0, RegistryValueKind.DWord),

        // Start, ads and nags
        new("Hide the Recommended section in Start", "HKLM", @"SOFTWARE\Policies\Microsoft\Windows\Explorer", "HideRecommendedSection", 1, RegistryValueKind.DWord),
        new("Hide the Recommended section in Start (policy manager)", "HKLM", @"SOFTWARE\Microsoft\PolicyManager\current\device\Start", "HideRecommendedSection", 1, RegistryValueKind.DWord),
        new("Enable the education flag that enforces Start policies", "HKLM", @"SOFTWARE\Microsoft\PolicyManager\current\device\Education", "IsEducationEnvironment", 1, RegistryValueKind.DWord),
        new("Disable Start menu recommendations", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_IrisRecommendations", 0, RegistryValueKind.DWord),
        new("Disable Start menu recent documents tracking", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackDocs", 0, RegistryValueKind.DWord),
        new("Hide recent items in the Start menu", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Start", "ShowRecentList", 0, RegistryValueKind.DWord),
        new("Hide frequent items in the Start menu", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Start", "ShowFrequentList", 0, RegistryValueKind.DWord),
        new("Disable web results in Start search", "HKCU", @"Software\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", 1, RegistryValueKind.DWord),
        new("Disable Bing in Start search", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Search", "BingSearchEnabled", 0, RegistryValueKind.DWord),
        new("Block automatically installed suggested apps", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SilentInstalledAppsEnabled", 0, RegistryValueKind.DWord),
        new("Disable Start menu subscribed suggestions", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338388Enabled", 0, RegistryValueKind.DWord),
        new("Disable Settings app suggestions", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SystemPaneSuggestionsEnabled", 0, RegistryValueKind.DWord),
        new("Disable advertising ID", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 0, RegistryValueKind.DWord),
        new("Dark mode for apps", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 0, RegistryValueKind.DWord),
        new("Dark mode for system", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", 0, RegistryValueKind.DWord),
        new("Keep Copilot disabled", "HKLM", @"SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", 1, RegistryValueKind.DWord),
        new("Keep Recall disabled", "HKLM", @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "AllowRecallEnablement", 0, RegistryValueKind.DWord),
        new("Disable lock screen spotlight", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenEnabled", 0, RegistryValueKind.DWord),
        new("Disable lock screen spotlight overlay", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenOverlayEnabled", 0, RegistryValueKind.DWord),
        new("Disable Start account notifications", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_AccountNotifications", 0, RegistryValueKind.DWord),
        new("Disable the finish-setting-up prompt", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\UserProfileEngagement", "ScoobeSystemSettingEnabled", 0, RegistryValueKind.DWord),
        new("Disable feedback requests", "HKLM", @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "DoNotShowFeedbackNotifications", 1, RegistryValueKind.DWord),
        new("Disable feedback prompts", "HKCU", @"SOFTWARE\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod", 0, RegistryValueKind.DWord),
        new("Disable cloud search", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\SearchSettings", "IsMSACloudSearchEnabled", 0, RegistryValueKind.DWord),
        new("Disable work cloud search", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\SearchSettings", "IsAADCloudSearchEnabled", 0, RegistryValueKind.DWord),
        new("Disable device search history", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\SearchSettings", "IsDeviceSearchHistoryEnabled", 0, RegistryValueKind.DWord),
        new("Disable search highlights", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\SearchSettings", "IsDynamicSearchBoxEnabled", 0, RegistryValueKind.DWord),
        new("Disable Cortana", "HKLM", @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCortana", 0, RegistryValueKind.DWord),
        new("Disable activity history publishing", "HKLM", @"SOFTWARE\Policies\Microsoft\Windows\System", "PublishUserActivities", 0, RegistryValueKind.DWord),
        new("Disable activity history upload", "HKLM", @"SOFTWARE\Policies\Microsoft\Windows\System", "UploadUserActivities", 0, RegistryValueKind.DWord),
        new("Lock the advertising ID by policy", "HKLM", @"SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo", "DisabledByGroupPolicy", 1, RegistryValueKind.DWord),
        new("Block device metadata downloads", "HKLM", @"SOFTWARE\Policies\Microsoft\Windows\Device Metadata", "PreventDeviceMetadataFromNetwork", 1, RegistryValueKind.DWord),
        new("Block Windows Platform Binary Table execution", "HKLM", @"SYSTEM\CurrentControlSet\Control\Session Manager", "DisableWpbtExecution", 1, RegistryValueKind.DWord),
        new("Disable content delivery", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "ContentDeliveryAllowed", 0, RegistryValueKind.DWord),
        new("Disable subscribed content", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContentEnabled", 0, RegistryValueKind.DWord),
        new("Disable feature management", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "FeatureManagementEnabled", 0, RegistryValueKind.DWord),
        new("Disable soft landing tips", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", 0, RegistryValueKind.DWord),
        new("Disable preinstalled apps", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "PreInstalledAppsEnabled", 0, RegistryValueKind.DWord),
        new("Keep preinstalled apps off", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "PreInstalledAppsEverEnabled", 0, RegistryValueKind.DWord),
        new("Disable OEM preinstalled apps", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "OemPreInstalledAppsEnabled", 0, RegistryValueKind.DWord),
        new("Disable welcome suggestions", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-310093Enabled", 0, RegistryValueKind.DWord),
        new("Disable Windows tips", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338389Enabled", 0, RegistryValueKind.DWord),
        new("Disable timeline suggestions", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338393Enabled", 0, RegistryValueKind.DWord),
        new("Disable Start tips", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353694Enabled", 0, RegistryValueKind.DWord),
        new("Disable Start tips (2)", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353696Enabled", 0, RegistryValueKind.DWord),

        // AI
        new("Disable AI data analysis", "HKLM", @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "DisableAIDataAnalysis", 1, RegistryValueKind.DWord),
        new("Disable saving AI snapshots", "HKLM", @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "TurnOffSavingSnapshots", 1, RegistryValueKind.DWord),
        new("Disable Click to Do", "HKLM", @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "DisableClickToDo", 1, RegistryValueKind.DWord),
        new("Disable the Settings agent", "HKLM", @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "DisableSettingsAgent", 1, RegistryValueKind.DWord),
        new("Disable agent connectors", "HKLM", @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "DisableAgentConnectors", 1, RegistryValueKind.DWord),
        new("Disable agent workspaces", "HKLM", @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "DisableAgentWorkspaces", 1, RegistryValueKind.DWord),
        new("Disable remote agent connectors", "HKLM", @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "DisableRemoteAgentConnectors", 1, RegistryValueKind.DWord),
        new("Disable the Copilot runtime", "HKLM", @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "AllowCopilotRuntime", 0, RegistryValueKind.DWord),
        new("Restrict app generative AI access", "HKLM", @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessGenerativeAI", 2, RegistryValueKind.DWord),
        new("Restrict app AI model access", "HKLM", @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsAccessSystemAIModels", 2, RegistryValueKind.DWord),
        new("Deny generative AI consent", "HKCU", @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\generativeAI", "Value", "Deny", RegistryValueKind.String),
        new("Disable Notepad AI features", "HKLM", @"SOFTWARE\Policies\WindowsNotepad", "DisableAIFeatures", 1, RegistryValueKind.DWord),
        new("Hide AI components in Settings", "HKLM", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "SettingsPageVisibility", "hide:aicomponents", RegistryValueKind.String),

        // Night light
        new("Night light schedule: sunset to sunrise", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\CloudStore\Store\DefaultAccount\Current\default$windows.data.bluelightreduction.settings\windows.data.bluelightreduction.settings", "Data", NightLightScheduleNow(), RegistryValueKind.Binary, IgnoreTimestamps: true),
        new("Night light on", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\CloudStore\Store\DefaultAccount\Current\default$windows.data.bluelightreduction.bluelightreductionstate\windows.data.bluelightreduction.bluelightreductionstate", "Data", NightLightStateNow(), RegistryValueKind.Binary, IgnoreTimestamps: true),

        // System
        new("Enable location services", "HKLM", @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location", "Value", "Allow", RegistryValueKind.String),
        new("Allow apps to access location", "HKCU", @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location", "Value", "Allow", RegistryValueKind.String),
        new("Deny desktop apps location access", "HKCU", @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location\NonPackaged", "Value", "Deny", RegistryValueKind.String),
        new("Allow the location sensor", "HKLM", @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Sensor\Overrides\{BFA794E4-F964-4FDB-90F6-51056BFE4B44}", "SensorPermissionState", 1, RegistryValueKind.DWord),
        new("Do not block location usage", "HKLM", @"SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableLocation", 0, RegistryValueKind.DWord),
        new("Store the hardware clock as UTC (dual boot; reboot)", "HKLM", @"SYSTEM\CurrentControlSet\Control\TimeZoneInformation", "RealTimeIsUniversal", 1, RegistryValueKind.DWord),
        new("Disable Fast Startup (dual boot; reboot)", "HKLM", @"SYSTEM\CurrentControlSet\Control\Session Manager\Power", "HiberbootEnabled", 0, RegistryValueKind.DWord),
        new("Lock when the screen turns off (reboot)", "HKLM", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "InactivityTimeoutSecs", 300, RegistryValueKind.DWord),
        new("Allow PowerShell scripts for Chezmoi hooks", "HKLM", @"SOFTWARE\Microsoft\PowerShell\1\ShellIds\Microsoft.PowerShell", "ExecutionPolicy", "RemoteSigned", RegistryValueKind.String),
        new("Allow PowerShell 7 scripts for Chezmoi hooks", "HKLM", @"SOFTWARE\Microsoft\PowerShellCore\ShellIds\Microsoft.PowerShell", "ExecutionPolicy", "RemoteSigned", RegistryValueKind.String),
        new("Pin feature updates to 25H2", "HKLM", @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", "TargetReleaseVersion", 1, RegistryValueKind.DWord),
        new("Pin feature updates to 25H2 (version)", "HKLM", @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", "TargetReleaseVersionInfo", "25H2", RegistryValueKind.String),
        new("Pin feature updates to 25H2 (product)", "HKLM", @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", "ProductVersion", "Windows 11", RegistryValueKind.String),
        new("Do not auto-reboot for updates", "HKLM", @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoRebootWithLoggedOnUsers", 1, RegistryValueKind.DWord),
        new("Keep Windows Update from replacing drivers", "HKLM", @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", "ExcludeWUDriversInQualityUpdate", 1, RegistryValueKind.DWord),
        new("Disable other Microsoft product updates", "HKLM", @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "AllowMUUpdateService", 0, RegistryValueKind.DWord),
        new("Do not opt into continuous innovation", "HKLM", @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings", "IsContinuousInnovationOptedIn", 0, RegistryValueKind.DWord),
        new("Disable Store app auto-updates", "HKLM", @"SOFTWARE\Policies\Microsoft\Windows\WindowsStore", "AutoDownload", 2, RegistryValueKind.DWord),
        new("Enable long file paths", "HKLM", @"SYSTEM\CurrentControlSet\Control\FileSystem", "LongPathsEnabled", 1, RegistryValueKind.DWord),
        new("Disable hibernation (reboot)", "HKLM", @"SYSTEM\CurrentControlSet\Control\Session Manager\Power", "HibernateEnabled", 0, RegistryValueKind.DWord),
        new("Disable the boot sound", "HKLM", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\LogonUI\BootAnimation", "DisableStartupSound", 1, RegistryValueKind.DWord),
        new("Disable the boot sound (edition override)", "HKLM", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\LogonUI\BootAnimation\EditionOverrides", "UserSetting_DisableStartupSound", 1, RegistryValueKind.DWord),

        // Gaming
        new("Disable Game DVR background recording", "HKCU", @"System\GameConfigStore", "GameDVR_Enabled", 0, RegistryValueKind.DWord),
        new("Disable app capture", "HKCU", @"Software\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", 0, RegistryValueKind.DWord),
        new("Enable Game Mode", "HKCU", @"Software\Microsoft\GameBar", "AutoGameModeEnabled", 1, RegistryValueKind.DWord),
        new("Allow automatic Game Mode", "HKCU", @"Software\Microsoft\GameBar", "AllowAutoGameMode", 1, RegistryValueKind.DWord),

        // Mouse
        new("Disable mouse acceleration speed", "HKCU", @"Control Panel\Mouse", "MouseSpeed", "0", RegistryValueKind.String),
        new("Disable mouse acceleration threshold 1", "HKCU", @"Control Panel\Mouse", "MouseThreshold1", "0", RegistryValueKind.String),
        new("Disable mouse acceleration threshold 2", "HKCU", @"Control Panel\Mouse", "MouseThreshold2", "0", RegistryValueKind.String),
        new("Make tooltips appear instantly", "HKCU", @"Control Panel\Mouse", "MouseHoverTime", "1", RegistryValueKind.String),
    ];

    public static byte[] NightLightScheduleNow() => BuildNightLightSchedule(NowUnix());

    public static byte[] NightLightStateNow()
    {
        var now = DateTimeOffset.UtcNow;
        var filetime = (ulong)((now.UtcDateTime - DateTime.UnixEpoch).Ticks + 116444736000000000L);
        return BuildNightLightState(NowUnix(), filetime);
    }

    public static byte[] BuildNightLightSchedule(ulong unixSeconds)
    {
        byte[] inner =
        [
            0x43, 0x42, 0x01, 0x00, 0x02, 0x01, 0xCA, 0x14, 0x00, 0xCA, 0x1E, 0x00, 0xCF, 0x28, 0x90,
            0x35, 0xCA, 0x32, 0x0E, 0x13, 0x2E, 0x17, 0x00, 0xCA, 0x3C, 0x0E, 0x07, 0x2E, 0x0C, 0x00,
            0x00,
        ];
        return Wrap(unixSeconds, inner);
    }

    public static byte[] BuildNightLightState(ulong unixSeconds, ulong filetime)
    {
        var inner = new List<byte> { 0x43, 0x42, 0x01, 0x00, 0x10, 0x00, 0xD0, 0x0A, 0x02, 0xC6, 0x14 };
        WriteVarint(inner, filetime);
        inner.Add(0x00);
        return Wrap(unixSeconds, [.. inner]);
    }

    private static byte[] Wrap(ulong unixSeconds, byte[] inner)
    {
        var blob = new List<byte> { 0x43, 0x42, 0x01, 0x00, 0x0A, 0x02, 0x01, 0x00, 0x2A, 0x06 };
        WriteVarint(blob, unixSeconds);
        blob.AddRange([0x2A, 0x2B, 0x0E, (byte)inner.Length]);
        blob.AddRange(inner);
        blob.AddRange([0x00, 0x00, 0x00]);
        return [.. blob];
    }

    private static void WriteVarint(ICollection<byte> target, ulong value)
    {
        while (value >= 0x80)
        {
            target.Add((byte)((value & 0x7F) | 0x80));
            value >>= 7;
        }

        target.Add((byte)value);
    }

    private static ulong NowUnix() => (ulong)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    private static byte[] NormalizeTimestamps(byte[] blob)
    {
        // ponytail: zeroes the envelope timestamp and the state FILETIME so the shell's own rewrites compare equal.
        var copy = (byte[])blob.Clone();
        ZeroVarintAfter(copy, 0x2A, 0x06);
        ZeroVarintAfter(copy, 0xC6, 0x14);
        return copy;
    }

    private static void ZeroVarintAfter(byte[] data, byte first, byte second)
    {
        for (var i = 0; i + 2 < data.Length; i++)
        {
            if (data[i] != first || data[i + 1] != second)
            {
                continue;
            }

            var j = i + 2;
            while (j < data.Length - 1 && (data[j] & 0x80) != 0)
            {
                data[j] = 0;
                j++;
            }

            data[j] = 0;
            return;
        }
    }

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
            RegistryValueKind.Binary when ByteIndex is null && IgnoreTimestamps => current is byte[] currentBlob
                && Value is byte[] targetBlob
                && NormalizeTimestamps(currentBlob).AsSpan().SequenceEqual(NormalizeTimestamps(targetBlob)),
            RegistryValueKind.Binary when ByteIndex is null => current is byte[] actual
                && Value is byte[] expected
                && actual.AsSpan().SequenceEqual(expected),
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
