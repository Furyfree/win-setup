using System.ComponentModel;
using System.Diagnostics;

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

    public static int RunInteractive(string file, params string[] args)
    {
        var startInfo = Create(file, args);
        startInfo.CreateNoWindow = false;
        try
        {
            using var process = Process.Start(startInfo) ?? throw new InvalidOperationException($"failed to start {file}");
            process.WaitForExit();
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
