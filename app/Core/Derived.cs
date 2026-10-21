namespace SPM.Core;

/// <summary>
/// What the coach learned for one attachment combination also helps the same weapon with other attachments: for every
/// combination without a profile of its own, the closest learned one is scaled with the same attachment factors the Lua uses.
/// Changing a grip or barrel therefore no longer throws the learning away.
/// </summary>
public static class Derived
{
    /// <summary>The learned guns say how strong the built-in estimates really need to be: each learned gun implies a strength
    /// (how much it ended up pulling compared with what the estimate pulled), the median of those is blended in as more guns are learned.
    /// An untrained gun therefore starts close to what your trained ones turned out to need instead of a fixed guess.</summary>
    public static (double Gain, int Guns, double Implied) EffectiveGain(AppConfig c)
    {
        double manual = Math.Clamp(c.Game.PullGain, 0.3, 5);
        var implied = new List<double>();
        foreach (var kv in c.Learned)
        {
            if (!c.Coach.Hist.TryGetValue(kv.Key, out var h) || h.BaseA <= 0.3) continue;
            var p = kv.Value;
            double b0 = p.R, b1 = p.R + p.Y1, b2 = b1 + p.Y2;
            double baseSum = h.BaseA + h.BaseB + h.BaseC;
            if (baseSum <= 1) continue;
            implied.Add(Math.Clamp(h.BaseGain * (b0 + b1 + b2) / baseSum, 0.4, 2.0));
        }
        if (!c.Game.AutoPullGain || implied.Count < 2) return (manual, implied.Count, 0);
        implied.Sort();
        double med = implied[implied.Count / 2];
        double w = implied.Count / (implied.Count + 2.0);
        return (Math.Round((manual * (1 - w) + med * w) / 0.05) * 0.05, implied.Count, med);
    }

    static double Att(string barrel, string grip) => (grip == "VERTICAL" ? 0.8 : 1) * (barrel == "FLASH HIDER" ? 0.8 : 1);
    // the Lua stores "no attachment" as the string NONE (never nil), so its keys read WEAPON:NONE:NONE
    static string Norm(string s) => s is "" or "-" ? "NONE" : s;

    public static IEnumerable<KeyValuePair<string, LearnedProfile>> All(AppConfig c)
    {
        foreach (var kv in c.Learned) yield return kv;
        var groups = c.Learned.Select(kv => (kv, p: kv.Key.Split(':'))).Where(x => x.p.Length == 3).GroupBy(x => x.p[0]);
        foreach (var g in groups)
        {
            var info = GameData.Weapon(g.Key);
            if (info == null) continue;
            var barrels = info.Barrels.Select(Norm).Append("NONE").Distinct().ToList();
            var grips = info.Grips.Select(Norm).Append("NONE").Distinct().ToList();
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
