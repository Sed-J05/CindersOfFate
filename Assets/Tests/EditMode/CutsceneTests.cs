using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Shatranj.Cutscenes;
using UnityEngine;

namespace Shatranj.Tests
{
    /// <summary>MiniJson basics and a lint of every cutscene script (known beat types, existing sprites / sets).</summary>
    public class CutsceneTests
    {
        private static readonly HashSet<string> KnownTypes = new HashSet<string>
        {
            "fade", "theme", "flame", "spawn", "anim", "move", "scale", "rotate", "tint", "alpha", "image", "remove", "text",
            "wait", "shake", "flash", "parallel", "sequence", "sfx", "music", "card", "invertwipe", "setflag", "loadscene",
        };

        [Test]
        public void MiniJson_ParsesNestedStructuresAndComments()
        {
            object root = MiniJson.Parse("{ // comment\n \"a\": [1, 2.5, -3e1], \"b\": {\"c\": \"x\\\"y\"}, \"d\": true, \"e\": null }");
            var obj = (Dictionary<string, object>)root;
            var a = (List<object>)obj["a"];
            Assert.AreEqual(3, a.Count);
            Assert.AreEqual(2.5, (double)a[1], 1e-9);
            Assert.AreEqual(-30.0, (double)a[2], 1e-9);
            Assert.AreEqual("x\"y", ((Dictionary<string, object>)obj["b"])["c"]);
            Assert.AreEqual(true, obj["d"]);
            Assert.IsNull(obj["e"]);
        }

        [Test]
        public void MiniJson_ReportsLineAndColumn()
        {
            var ex = Assert.Throws<MiniJson.ParseException>(() => MiniJson.Parse("{\n  \"a\": 1,\n  \"b\" 2\n}"));
            StringAssert.Contains("line 3", ex.Message);
        }

        [Test]
        public void Beat_TypedGettersHaveDefaults()
        {
            var beat = new Beat((Dictionary<string, object>)MiniJson.Parse("{\"type\":\"Move\",\"to\":[1.5,-2],\"time\":0.5,\"colour\":\"#FF0000\"}"));
            Assert.AreEqual("move", beat.Type);
            Assert.AreEqual(new Vector2(1.5f, -2f), beat.Vec2("to"));
            Assert.AreEqual(0.5f, beat.Float("time"));
            Assert.AreEqual(3f, beat.Float("missing", 3f));
            Assert.AreEqual(Color.red, beat.Colour("colour", Color.white));
            Assert.IsFalse(beat.Async);
        }

        [TestCase("cs1_intro")]
        [TestCase("cs2_orb")]
        public void CutsceneScript_IsValid(string id)
        {
            string path = Path.Combine(Application.dataPath, "Data/Cutscenes", id + ".json");
            Assert.IsTrue(File.Exists(path), path);
            CutsceneScript script = CutsceneScript.Parse(File.ReadAllText(path), id);
            Assert.AreEqual(id, script.id);
            Assert.Greater(script.beats.Count, 5);
            bool hasLoad = false;
            Lint(script.beats, ref hasLoad);
            Assert.IsTrue(hasLoad, $"{id} should end with a loadScene beat");
        }

        private static void Lint(List<Beat> beats, ref bool hasLoad)
        {
            foreach (Beat b in beats)
            {
                Assert.IsTrue(KnownTypes.Contains(b.Type), $"unknown beat type '{b.Type}'");
                if (b.Type == "loadscene") hasLoad = true;
                if (b.Type == "image")
                {
                    string s = b.Str("sprite");
                    bool exists = File.Exists($"Assets/Art/Cutscenes/{s}.png") || File.Exists($"Assets/Art/UI/{s}.png") || File.Exists($"Assets/Art/Backgrounds/{s}.png");
                    Assert.IsTrue(exists, $"image sprite '{s}' not found in Art/Cutscenes, UI or Backgrounds");
                }
                if (b.Type == "spawn" && b.Has("character"))
                    Assert.IsTrue(File.Exists($"Assets/Resources/Animations/{b.Str("character")}.asset"), $"animation set '{b.Str("character")}' missing");
                if (b.Type == "parallel" || b.Type == "sequence") Lint(b.Beats("beats"), ref hasLoad);
            }
        }
    }
}
