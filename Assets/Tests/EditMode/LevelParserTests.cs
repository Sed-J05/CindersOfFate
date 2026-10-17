using System.Collections.Generic;
using NUnit.Framework;
using Shatranj.Level;
using UnityEngine;

namespace Shatranj.Tests
{
    public class LevelParserTests
    {
        private const string Valid =
            "# id: T1\n" +
            "# size: 8x5\n" +
            "# origin: 16,2\n" +
            "# theme: cave\n" +
            "# name: Test Room\n" +
            "########\n" +
            "#..c...#\n" +
            "#.P..=.#\n" +
            "#...x..#\n" +
            "########\n";

        [Test]
        public void ParsesHeaderAndSize()
        {
            RoomData r = LevelParser.ParseRoom(Valid, "T1.txt");
            Assert.AreEqual("T1", r.id);
            Assert.AreEqual(8, r.width);
            Assert.AreEqual(5, r.height);
            Assert.AreEqual(16, r.originX);
            Assert.AreEqual(2, r.originY);
            Assert.AreEqual("cave", r.theme);
            Assert.AreEqual("Test Room", r.name);
        }

        [Test]
        public void TopRowOfFileIsHighestY()
        {
            RoomData r = LevelParser.ParseRoom(Valid, "T1.txt");
            Assert.AreEqual('#', r.cells[0, 4]);          // top-left corner
            Assert.AreEqual('c', r.cells[3, 3]);          // second line, col 3
            Assert.AreEqual('P', r.cells[2, 2]);
            Assert.AreEqual('=', r.cells[5, 2]);
            Assert.AreEqual('x', r.cells[4, 1]);
            Assert.IsTrue(r.IsSolid(-1, 0), "outside the room counts as solid");
        }

        [Test]
        public void FindReturnsReadingOrder()
        {
            RoomData r = LevelParser.ParseRoom(Valid, "T1.txt");
            List<Vector2Int> p = r.Find('P');
            Assert.AreEqual(1, p.Count);
            Assert.AreEqual(new Vector2Int(2, 2), p[0]);
            Assert.AreEqual(1, r.CountEnemies());
        }

        [Test]
        public void WorldPositionsUseHalfUnitTiles()
        {
            RoomData r = LevelParser.ParseRoom(Valid, "T1.txt");
            Vector2 bl = r.CellBottomLeft(0, 0);
            Assert.AreEqual(8f, bl.x, 1e-5f);
            Assert.AreEqual(1f, bl.y, 1e-5f);
            Assert.AreEqual(new Rect(8f, 1f, 4f, 2.5f), r.WorldRect);
        }

        [Test]
        public void WrongRowCount_IsAnError()
        {
            string text = Valid.Replace("########\n#...x..#\n", "########\n");
            var ex = Assert.Throws<LevelParseException>(() => LevelParser.ParseRoom(text, "T1.txt"));
            StringAssert.Contains("rows", ex.Message);
        }

        [Test]
        public void RowTooLong_IsAnErrorWithLineNumber()
        {
            string text = Valid.Replace("#..c...#", "#..c....#");
            var ex = Assert.Throws<LevelParseException>(() => LevelParser.ParseRoom(text, "T1.txt"));
            StringAssert.Contains("line 7", ex.Message);
            StringAssert.Contains("expected 8", ex.Message);
        }

        [Test]
        public void ShortRow_IsPaddedWithWarning()
        {
            string text = Valid.Replace("#...x..#", "#...x..");
            var warnings = new List<string>();
            RoomData r = LevelParser.ParseRoom(text, "T1.txt", warnings);
            Assert.AreEqual(1, warnings.Count);
            Assert.AreEqual('.', r.cells[7, 1]);
        }

        [Test]
        public void UnknownGlyph_IsAnErrorWithPosition()
        {
            string text = Valid.Replace("#..c...#", "#..q...#");
            var ex = Assert.Throws<LevelParseException>(() => LevelParser.ParseRoom(text, "T1.txt"));
            StringAssert.Contains("unknown glyph 'q'", ex.Message);
            StringAssert.Contains("col 4", ex.Message);
        }

        [Test]
        public void MissingSize_IsAnError()
        {
            string text = Valid.Replace("# size: 8x5\n", "");
            var ex = Assert.Throws<LevelParseException>(() => LevelParser.ParseRoom(text, "T1.txt"));
            StringAssert.Contains("size", ex.Message);
        }

        [Test]
        public void TwoPlayerStarts_IsAnError()
        {
            string text = Valid.Replace("#...x..#", "#.P.x..#");
            Assert.Throws<LevelParseException>(() => LevelParser.ParseRoom(text, "T1.txt"));
        }

        [Test]
        public void SpacesCountAsEmpty()
        {
            string text = Valid.Replace("#..c...#", "#  c   #");
            RoomData r = LevelParser.ParseRoom(text, "T1.txt");
            Assert.AreEqual('.', r.cells[1, 3]);
            Assert.AreEqual('c', r.cells[3, 3]);
        }

        [Test]
        public void ManifestParsesSignsAndDoors()
        {
            const string json = "{\"version\":1,\"phases\":[{\"id\":\"P1\",\"phase\":1,\"scene\":\"Phase1_Cave\",\"theme\":\"cave\"," +
                                "\"rooms\":[{\"id\":\"P1_A0\",\"file\":\"P1_A0.txt\",\"section\":\"A\"}]," +
                                "\"sections\":[{\"id\":\"A\",\"expectedEnemies\":15,\"bossId\":\"kage_warden\"}]}]," +
                                "\"signs\":[{\"room\":\"P1_A0\",\"index\":0,\"text\":\"A / D — move\"}]," +
                                "\"doors\":[{\"room\":\"P1_MB2\",\"flag\":\"door_orb\"}]}";
            LevelManifest m = LevelParser.ParseManifest(json);
            Assert.AreEqual(1, m.phases.Count);
            Assert.AreEqual(15, m.phases[0].FindSection("A").expectedEnemies);
            Assert.AreEqual("A / D — move", m.SignText("P1_A0", 0));
            Assert.IsNull(m.SignText("P1_A0", 1));
            Assert.AreEqual("door_orb", m.DoorFlag("P1_MB2"));
            Assert.AreEqual("door_p1_x", m.DoorFlag("P1_X"));
        }

        [Test]
        public void BitmaskIndex_MatchesDocs06()
        {
            // 3×3 room: centre cell solid with solid N and W neighbours only.
            string text = "# id: B\n# size: 3x3\n# origin: 0,0\n" + ".#.\n##.\n...\n";
            RoomData r = LevelParser.ParseRoom(text, "B.txt");
            int idx = Shatranj.EditorTools.Level.LevelBuilder.BitmaskIndex(r, 1, 1);
            Assert.AreEqual(1 + 8, idx); // N=1, W=8
        }
    }
}
