namespace WinSetup;

public static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length != 1)
        {
            return Usage();
        }

        return args[0].ToLowerInvariant() switch
        {
            "status" => Commands.Status(),
            "apply" => Update.RunApply(args),
            "snapshot" => Snapshot.Run(),
            "version" => Version(),
            _ => Usage(),
        };
    }

    private static int Version()
    {
        Console.WriteLine($"win-setup {Update.DisplayVersion}");
        return 0;
    }

    private static int Usage()
    {
        Console.WriteLine("usage: win-setup status|apply|snapshot|version");
        return 2;
    }
}
