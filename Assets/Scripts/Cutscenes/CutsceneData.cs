using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Shatranj.Cutscenes
{
    /// <summary>
    /// One beat of a cutscene script (Docs/08 §1). Wraps the parsed JSON object with typed, defaulted getters so every
    /// beat type can read just the fields it needs. Common optional fields: <c>async</c> (don't wait for this beat),
    /// <c>minCycle</c> / <c>maxCycle</c> (run only on those cycles).
    /// </summary>
    public class Beat
    {
        private readonly Dictionary<string, object> data;

        public Beat(Dictionary<string, object> source) { data = source ?? new Dictionary<string, object>(); }

        public string Type => Str("type", "").ToLowerInvariant();
        public bool Async => Bool("async", false);
        public int MinCycle => Int("minCycle", 0);
        public int MaxCycle => Int("maxCycle", int.MaxValue);

        public bool Has(string key) => data.ContainsKey(key) && data[key] != null;
        public object Raw(string key) => data.TryGetValue(key, out object v) ? v : null;

        public string Str(string key, string fallback = null)
        {
            if (!data.TryGetValue(key, out object v) || v == null) return fallback;
            if (v is string s) return s;
            if (v is double d) return d.ToString(CultureInfo.InvariantCulture);
            if (v is bool b) return b ? "true" : "false";
            return fallback;
        }

        public float Float(string key, float fallback = 0f)
        {
            if (!data.TryGetValue(key, out object v) || v == null) return fallback;
            if (v is double d) return (float)d;
            if (v is string s && float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float f)) return f;
            if (v is bool b) return b ? 1f : 0f;
            return fallback;
        }

        public int Int(string key, int fallback = 0) => Has(key) ? Mathf.RoundToInt(Float(key, fallback)) : fallback;

        public bool Bool(string key, bool fallback = false)
        {
            if (!data.TryGetValue(key, out object v) || v == null) return fallback;
            if (v is bool b) return b;
            if (v is double d) return d != 0d;
            if (v is string s) return s == "true" || s == "1" || s == "yes";
            return fallback;
        }

        /// <summary>[x, y] array → Vector2.</summary>
        public Vector2 Vec2(string key, Vector2 fallback = default)
        {
            if (!(Raw(key) is List<object> list) || list.Count < 2) return fallback;
            return new Vector2(ToFloat(list[0]), ToFloat(list[1]));
        }

        public List<string> Strings(string key)
        {
            var result = new List<string>();
            object v = Raw(key);
            if (v is string single) { result.Add(single); return result; }
            if (v is List<object> list) foreach (object o in list) if (o != null) result.Add(o.ToString());
            return result;
        }

        public List<Beat> Beats(string key)
        {
            var result = new List<Beat>();
            if (Raw(key) is List<object> list)
                foreach (object o in list) if (o is Dictionary<string, object> d) result.Add(new Beat(d));
            return result;
        }

        public Beat Child(string key) => Raw(key) is Dictionary<string, object> d ? new Beat(d) : null;

        /// <summary>"#RRGGBB(AA)", a colour name (white, black, red, blue, purple, orange, parchment) or [r, g, b(, a)].</summary>
        public Color Colour(string key, Color fallback)
        {
            object v = Raw(key);
            if (v is List<object> list && list.Count >= 3)
                return new Color(ToFloat(list[0]), ToFloat(list[1]), ToFloat(list[2]), list.Count > 3 ? ToFloat(list[3]) : 1f);
            if (!(v is string s)) return fallback;
            switch (s.ToLowerInvariant())
            {
                case "white": return Color.white;
                case "black": return Color.black;
                case "red": return new Color(1f, 0.16f, 0.23f);
                case "blue": return new Color(0.24f, 0.63f, 1f);
                case "purple": return new Color(0.61f, 0.30f, 1f);
                case "orange": return new Color(1f, 0.54f, 0.17f);
                case "parchment": return new Color(0.91f, 0.85f, 0.71f);
            }
            return ColorUtility.TryParseHtmlString(s, out Color c) ? c : fallback;
        }

        public static float ToFloat(object o)
        {
            if (o is double d) return (float)d;
            if (o is string s && float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float f)) return f;
            return 0f;
        }

        public override string ToString() => $"{Type} {Str("id", "")}".Trim();
    }

    /// <summary>A whole cutscene file: id, starting theme, skip rules and the beat list.</summary>
    public class CutsceneScript
    {
        public string id = "";
        public string theme = "";
        public float skipHoldTime = 1f;
        /// <summary>From this cycle on, a single press skips (Docs/08 §2: cutscene 1 on cycles ≥ 2).</summary>
        public int instantSkipFromCycle = int.MaxValue;
        public readonly List<Beat> beats = new List<Beat>();

        public static CutsceneScript Parse(string json, string fallbackId)
        {
            object root = MiniJson.Parse(json);
            if (!(root is Dictionary<string, object> obj)) throw new MiniJson.ParseException("cutscene root must be an object");
            var head = new Beat(obj);
            var script = new CutsceneScript
            {
                id = head.Str("id", fallbackId),
                theme = head.Str("theme", ""),
                skipHoldTime = head.Float("skipHoldTime", 1f),
                instantSkipFromCycle = head.Int("instantSkipFromCycle", int.MaxValue),
            };
            script.beats.AddRange(head.Beats("beats"));
            return script;
        }
    }
}
