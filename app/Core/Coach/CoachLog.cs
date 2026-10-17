namespace SPM.Core.Coach;

/// <summary>Everything the coach does, one line each, in %APPDATA%\\SiegeProfileManager\\coach.log (kept across restarts, rotated at 3 MB).</summary>
public static class CoachLog
{
    static readonly object Gate = new();
    public static string Path { get; set; } = "";

    public static void Append(string line)
    {
        if (Path == "") return;
        try
        {
            lock (Gate)
            {
                var fi = new FileInfo(Path);
                if (fi.Exists && fi.Length > 3_000_000) File.Move(Path, Path + ".old", true);
                File.AppendAllText(Path, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + line + "\n");
            }
        }
        catch { }
    }

    public static string Tail(int lines)
    {
        try { lock (Gate) return File.Exists(Path) ? string.Join("\n", File.ReadLines(Path).TakeLast(lines)) : ""; } catch { return ""; }
    }
}
