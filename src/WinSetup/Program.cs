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
            "apply" => Apply(args),
            "snapshot" => Snapshot.Run(),
            "version" => Version(),
            _ => Usage(),
        };
    }

    private static int Apply(string[] args)
    {
        if (!OperatingSystem.IsWindows())
        {
            Console.WriteLine("apply must run on Windows");
            return 2;
        }

        if (Update.RestartIfNewer(args) is int exitCode)
        {
            return exitCode;
        }

        return WithLog(Update.RunApply, args, Path.Combine(Paths.LocalAppData, "win-setup", "apply.log"));
    }

    public static int WithLog(Func<string[], int> action, string[] args, string path)
    {
        StreamWriter file;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            file = new StreamWriter(path, append: false) { AutoFlush = true };
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"Cannot open apply log: {exception.Message}");
            return action(args);
        }

        using (file)
        {
            var console = Console.Out;
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
