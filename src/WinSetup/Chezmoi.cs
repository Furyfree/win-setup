namespace WinSetup;

public static class Chezmoi
{
    public const string InitHint = "chezmoi init --apply git@github.com:Furyfree/dotfiles.git";

    public static bool IsInitialized()
    {
        var result = Runner.Run(Paths.Chezmoi, ["source-path"]);
        return result.Ok && Directory.Exists(result.StdOut.Trim());
    }

    public static int Apply() => Runner.RunInteractive(Paths.Chezmoi, "apply");
}
