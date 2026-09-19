using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WinSetup;

public record RunResult(int ExitCode, string StdOut, string StdErr)
{
    public bool Ok => ExitCode == 0;

    public string Hex => $"0x{unchecked((uint)ExitCode):X8}";
}

public static class Runner
{
    public static RunResult Run(string file, string[] args, IReadOnlyDictionary<string, string>? environment = null)
    {
        var startInfo = Create(file, args);
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        if (environment is not null)
        {
            foreach (var (key, value) in environment)
            {
                startInfo.Environment[key] = value;
            }
        }

        try
        {
            using var process = Process.Start(startInfo) ?? throw new InvalidOperationException($"failed to start {file}");
            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderr = process.StandardError.ReadToEndAsync();
            process.WaitForExit();
            return new(process.ExitCode, stdout.Result, stderr.Result);
        }
        catch (Win32Exception exception)
        {
            return new(-1, string.Empty, exception.Message);
        }
    }

    public static int RunInteractive(string file, string[] args, string? label = null)
    {
        var startInfo = Create(file, args);
        startInfo.CreateNoWindow = false;
        try
        {
            using var process = Process.Start(startInfo) ?? throw new InvalidOperationException($"failed to start {file}");
            if (label is null || Console.IsErrorRedirected)
            {
                process.WaitForExit();
                RestoreConsole();
                return process.ExitCode;
            }

            var stopwatch = Stopwatch.StartNew();
            var spinner = Task.Run(async () =>
            {
                while (!process.HasExited)
                {
                    Console.Error.Write(("\r      " + $"{label} {stopwatch.Elapsed.TotalSeconds:0}s...").PadRight(79));
                    await Task.Delay(500);
                }
            });
            process.WaitForExit();
            spinner.Wait();
            Console.Error.Write("\r".PadRight(80) + "\r");
            RestoreConsole();
            return process.ExitCode;
        }
        catch (Win32Exception)
        {
            return -1;
        }
    }

    public static RunResult RunStreaming(string file, string[] args, string prefix = "      ")
    {
        var startInfo = Create(file, args);
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        try
        {
            using var process = Process.Start(startInfo) ?? throw new InvalidOperationException($"failed to start {file}");
            var stdout = new System.Text.StringBuilder();
            var stderr = new System.Text.StringBuilder();
            process.OutputDataReceived += (_, line) =>
            {
                if (line.Data is null)
                {
                    return;
                }

                stdout.AppendLine(line.Data);
                Console.WriteLine(prefix + line.Data);
            };
            process.ErrorDataReceived += (_, line) =>
            {
                if (line.Data is null)
                {
                    return;
                }

                stderr.AppendLine(line.Data);
                Console.WriteLine(prefix + line.Data);
            };
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            process.WaitForExit();
            return new(process.ExitCode, stdout.ToString(), stderr.ToString());
        }
        catch (Win32Exception exception)
        {
            return new(-1, string.Empty, exception.Message);
        }
    }

    public static string? Find(params string[] candidates) => candidates.FirstOrDefault(File.Exists);

    // ponytail: winget leaves the console input mode altered; PSReadLine then waits for a key before drawing the prompt.
    public static void RestoreConsole()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        try
        {
            var input = GetStdHandle(-10);
            if (GetConsoleMode(input, out var inputMode))
            {
                SetConsoleMode(input, inputMode | 0x0001 | 0x0002 | 0x0004);
                FlushConsoleInputBuffer(input);
            }

            var output = GetStdHandle(-11);
            if (GetConsoleMode(output, out var outputMode))
            {
                SetConsoleMode(output, outputMode | 0x0004);
            }
        }
        catch (Exception)
        {
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int handle);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetConsoleMode(IntPtr handle, out uint mode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleMode(IntPtr handle, uint mode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool FlushConsoleInputBuffer(IntPtr handle);

    private static ProcessStartInfo Create(string file, string[] args)
    {
        var startInfo = new ProcessStartInfo(file)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        foreach (var arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }

        return startInfo;
    }
}
