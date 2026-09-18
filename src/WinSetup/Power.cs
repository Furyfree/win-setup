using System.Text.RegularExpressions;

namespace WinSetup;

public record PowerItem(string Why, string Subgroup, string Setting, uint Ac)
{
    public static readonly PowerItem[] All =
    [
        new("Turn off the display after 5 minutes", "SUB_VIDEO", "VIDEOIDLE", 300),
        new("Never sleep while plugged in", "SUB_SLEEP", "STANDBYIDLE", 0),
        new("Power button does nothing while plugged in", "SUB_BUTTONS", "PBUTTONACTION", 0),
    ];

    public uint? ReadAc() => Power.QueryAc(Subgroup, Setting);

    public RunResult Write() => Power.SetAc(Subgroup, Setting, Ac);
}

public static partial class Power
{
    public static string Exe => Path.Combine(Environment.SystemDirectory, "powercfg.exe");

    public static uint? QueryAc(string subgroup, string setting) =>
        ParseAc(Runner.Run(Exe, ["/query", "SCHEME_CURRENT", subgroup, setting]).StdOut);

    public static uint? ParseAc(string output)
    {
        var matches = Hex().Matches(output);
        return matches.Count >= 2 ? Convert.ToUInt32(matches[^2].Value, 16) : null;
    }

    public static RunResult SetAc(string subgroup, string setting, uint value)
    {
        var set = Runner.Run(Exe, ["/setacvalueindex", "SCHEME_CURRENT", subgroup, setting, value.ToString()]);
        return set.Ok ? Runner.Run(Exe, ["/setactive", "SCHEME_CURRENT"]) : set;
    }

    public static RunResult DisableHibernate() => Runner.Run(Exe, ["/hibernate", "off"]);

    [GeneratedRegex(@"0x[0-9a-fA-F]{8}")]
    private static partial Regex Hex();
}
