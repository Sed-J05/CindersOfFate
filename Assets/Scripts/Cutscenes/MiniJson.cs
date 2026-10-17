using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Shatranj.Cutscenes
{
    /// <summary>
    /// Small, dependency-free JSON reader (D-138): objects → Dictionary&lt;string, object&gt;, arrays → List&lt;object&gt;,
    /// numbers → double, plus string / bool / null. Errors name the line and column. Comments (// …) are allowed so
    /// writers can annotate cutscene scripts.
    /// </summary>
    public static class MiniJson
    {
        public class ParseException : Exception
        {
            public ParseException(string message) : base(message) { }
        }

        public static object Parse(string json)
        {
            if (json == null) throw new ParseException("JSON text is null");
            var p = new Parser(json);
            p.SkipWhitespace();
            object value = p.ParseValue();
            p.SkipWhitespace();
            if (!p.AtEnd) p.Fail("unexpected text after the end of the document");
            return value;
        }

        private sealed class Parser
        {
            private readonly string s;
            private int i;

            public Parser(string text) { s = text; }

            public bool AtEnd => i >= s.Length;

            public void Fail(string message)
            {
                int line = 1, col = 1;
                for (int k = 0; k < i && k < s.Length; k++)
                {
                    if (s[k] == '\n') { line++; col = 1; } else col++;
                }
                throw new ParseException($"JSON line {line}, col {col}: {message}");
            }

            public void SkipWhitespace()
            {
                while (i < s.Length)
                {
                    char c = s[i];
                    if (char.IsWhiteSpace(c)) { i++; continue; }
                    if (c == '/' && i + 1 < s.Length && s[i + 1] == '/')
                    {
                        while (i < s.Length && s[i] != '\n') i++;
                        continue;
                    }
                    break;
                }
            }

            public object ParseValue()
            {
                if (AtEnd) Fail("unexpected end of text");
                char c = s[i];
                switch (c)
                {
                    case '{': return ParseObject();
                    case '[': return ParseArray();
                    case '"': return ParseString();
                    case 't': Expect("true"); return true;
                    case 'f': Expect("false"); return false;
                    case 'n': Expect("null"); return null;
                    default:
                        if (c == '-' || char.IsDigit(c)) return ParseNumber();
                        Fail($"unexpected character '{c}'");
                        return null;
                }
            }

            private void Expect(string word)
            {
                if (string.CompareOrdinal(s, i, word, 0, word.Length) != 0) Fail($"expected '{word}'");
                i += word.Length;
            }

            private Dictionary<string, object> ParseObject()
            {
                var obj = new Dictionary<string, object>();
                i++; // {
                SkipWhitespace();
                if (!AtEnd && s[i] == '}') { i++; return obj; }
                while (true)
                {
                    SkipWhitespace();
                    if (AtEnd || s[i] != '"') Fail("expected a property name in quotes");
                    string key = ParseString();
                    SkipWhitespace();
                    if (AtEnd || s[i] != ':') Fail($"expected ':' after \"{key}\"");
                    i++;
                    SkipWhitespace();
                    obj[key] = ParseValue();
                    SkipWhitespace();
                    if (AtEnd) Fail("unterminated object");
                    if (s[i] == ',') { i++; SkipWhitespace(); if (!AtEnd && s[i] == '}') { i++; return obj; } continue; }
                    if (s[i] == '}') { i++; return obj; }
                    Fail("expected ',' or '}'");
                }
            }

            private List<object> ParseArray()
            {
                var list = new List<object>();
                i++; // [
                SkipWhitespace();
                if (!AtEnd && s[i] == ']') { i++; return list; }
                while (true)
                {
                    SkipWhitespace();
                    list.Add(ParseValue());
                    SkipWhitespace();
                    if (AtEnd) Fail("unterminated array");
                    if (s[i] == ',') { i++; SkipWhitespace(); if (!AtEnd && s[i] == ']') { i++; return list; } continue; }
                    if (s[i] == ']') { i++; return list; }
                    Fail("expected ',' or ']'");
                }
            }

            private string ParseString()
            {
                var sb = new StringBuilder();
                i++; // opening quote
                while (true)
                {
                    if (AtEnd) Fail("unterminated string");
                    char c = s[i++];
                    if (c == '"') return sb.ToString();
                    if (c != '\\') { sb.Append(c); continue; }
                    if (AtEnd) Fail("unterminated escape");
                    char e = s[i++];
                    switch (e)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            if (i + 4 > s.Length) Fail("bad \\u escape");
                            sb.Append((char)Convert.ToInt32(s.Substring(i, 4), 16));
                            i += 4;
                            break;
                        default: Fail($"unknown escape '\\{e}'"); break;
                    }
                }
            }

            private double ParseNumber()
            {
                int start = i;
                if (s[i] == '-') i++;
                while (i < s.Length && (char.IsDigit(s[i]) || s[i] == '.' || s[i] == 'e' || s[i] == 'E' || s[i] == '+' || s[i] == '-')) i++;
                string token = s.Substring(start, i - start);
                if (!double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out double d)) Fail($"bad number '{token}'");
                return d;
            }
        }
    }
}
