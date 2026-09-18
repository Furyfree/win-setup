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
            "apply" => WithLog(Update.RunApply, args),
            "snapshot" => Snapshot.Run(),
            "version" => Version(),
            _ => Usage(),
        };
    }

    private static int WithLog(Func<string[], int> action, string[] args)
    {
        var path = Path.Combine(Paths.LocalAppData, "win-setup", "apply.log");
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var console = Console.Out;
            using var file = new StreamWriter(path, append: false) { AutoFlush = true };
            Console.SetOut(new TeeWriter(console, file));
            try
            {
                Console.WriteLine($"win-setup {Update.DisplayVersion} apply {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz}");
                return action(args);
            }
            finally
            {
                Console.SetOut(console);
                Console.WriteLine($"log: {path}");
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return action(args);
        }
    }

    private sealed class TeeWriter(TextWriter first, TextWriter second) : TextWriter
    {
        public override System.Text.Encoding Encoding => first.Encoding;

        public override void Write(char value)
        {
            first.Write(value);
            second.Write(value);
        }

        public override void Write(string? value)
        {
            first.Write(value);
            second.Write(value);
        }
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
