namespace SPM.Core;

/// <summary>
/// What the coach learned for one attachment combination also helps the same weapon with other attachments: for every
/// combination without a profile of its own, the closest learned one is scaled with the same attachment factors the Lua uses.
/// Changing a grip or barrel therefore no longer throws the learning away.
/// </summary>
public static class Derived
{
    static double Att(string barrel, string grip) => (grip == "VERTICAL" ? 0.8 : 1) * (barrel == "FLASH HIDER" ? 0.8 : 1);
    static string Norm(string s) => s is "" or "NONE" or "-" ? "nil" : s;

    public static IEnumerable<KeyValuePair<string, LearnedProfile>> All(AppConfig c)
    {
        foreach (var kv in c.Learned) yield return kv;
        var groups = c.Learned.Select(kv => (kv, p: kv.Key.Split(':'))).Where(x => x.p.Length == 3).GroupBy(x => x.p[0]);
        foreach (var g in groups)
        {
            var info = GameData.Weapon(g.Key);
            if (info == null) continue;
            var barrels = info.Barrels.Select(Norm).Append("nil").Distinct().ToList();
            var grips = info.Grips.Select(Norm).Append("nil").Distinct().ToList();
            foreach (var b in barrels)
                foreach (var gr in grips)
                {
                    string key = $"{g.Key}:{b}:{gr}";
                    if (c.Learned.ContainsKey(key)) continue;
                    var src = g.OrderByDescending(x => (x.p[1] == b ? 2 : 0) + (x.p[2] == gr ? 1 : 0)).First();
                    double k = Att(b, gr) / Att(src.p[1], src.p[2]);
                    var p = src.kv.Value;
                    yield return new(key, new LearnedProfile { R = Math.Round(p.R * k, 3), Y1 = Math.Round(p.Y1 * k, 3), Y2 = Math.Round(p.Y2 * k, 3), Tym1 = p.Tym1, Tym2 = p.Tym2, Side = p.Side, Strength = p.Strength, Late = p.Late });
                }
        }
    }
}
