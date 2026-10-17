using System.Collections.Generic;
using NUnit.Framework;
using Shatranj.EditorTools.Level;
using Shatranj.Level;

namespace Shatranj.Tests
{
    /// <summary>Prompt 14: Section D rooms (Docs/05 §5) read from the real room files.</summary>
    public class SectionDTests
    {
        private static readonly string[] Order =
            { "P2_D1", "P2_D2", "P2_D3", "P2_D4", "P2_D5", "P2_D6", "P2_D7", "P2_D8", "P2_THRONE" };

        private static Dictionary<string, RoomData> Rooms(out LevelSet set, out LevelManifestPhase p2)
        {
            set = LevelParser.Load(LevelBuilder.MainManifestPath, LevelBuilder.LevelsFolder);
            Assert.IsEmpty(set.errors, string.Join("\n", set.errors));
            p2 = set.manifest.phases.Find(p => p.id == "P2");
            Assert.IsNotNull(p2);
            var map = new Dictionary<string, RoomData>();
            foreach (RoomData r in set.RoomsOfPhase(p2)) map[r.id] = r;
            return map;
        }

        private static int Count(RoomData room, char glyph)
        {
            int n = 0;
            for (int y = 0; y < room.height; y++)
                for (int x = 0; x < room.width; x++)
                    if (room.cells[x, y] == glyph) n++;
            return n;
        }

        [Test]
        public void SectionD_MatchesDocTable_AndPhase2HasFifty()
        {
            var rooms = Rooms(out _, out LevelManifestPhase p2);
            int[] expected = { 4, 4, 4, 4, 4, 4, 3, 3, 0 };
            int d = 0;
            for (int i = 0; i < Order.Length; i++)
            {
                Assert.IsTrue(rooms.ContainsKey(Order[i]), Order[i] + " missing from the manifest");
                Assert.AreEqual("D", rooms[Order[i]].section, Order[i]);
                Assert.AreEqual(expected[i], rooms[Order[i]].CountEnemies(), Order[i]);
                d += rooms[Order[i]].CountEnemies();
            }
            Assert.AreEqual(30, d);

            int total = 0;
            foreach (RoomData r in rooms.Values) total += r.CountEnemies();
            Assert.AreEqual(50, total, "Phase 2 regular enemies (Docs/05 §5)");
        }

        [Test]
        public void ChargerIntroducedInD1_ThrowerInD2()
        {
            var rooms = Rooms(out _, out _);
            string[] route = { "P2_C0", "P2_C1", "P2_C2", "P2_C3", "P2_C4", "P2_C5", "P2_C6", "P2_MB",
                               "P2_D1", "P2_D2", "P2_D3", "P2_D4", "P2_D5", "P2_D6", "P2_D7", "P2_D8", "P2_THRONE" };
            string firstCharger = null, firstThrower = null;
            foreach (string id in route)
            {
                if (firstCharger == null && Count(rooms[id], Glyphs.Charger) > 0) firstCharger = id;
                if (firstThrower == null && Count(rooms[id], Glyphs.Thrower) > 0) firstThrower = id;
            }
            Assert.AreEqual("P2_D1", firstCharger);
            Assert.AreEqual("P2_D2", firstThrower);
            Assert.AreEqual(1, Count(rooms["P2_D1"], Glyphs.Charger), "D1 introduces a single charger");
            Assert.AreEqual(1, Count(rooms["P2_D2"], Glyphs.Thrower), "D2 introduces a single thrower");
        }

        [Test]
        public void StaticLava_OnlyInD6_AndAlwaysOnAFloor()
        {
            var rooms = Rooms(out _, out _);
            foreach (RoomData room in rooms.Values)
            {
                int lava = Count(room, Glyphs.Lava);
                if (room.id == "P2_D6") Assert.Greater(lava, 0, "D6 has the static lava pits");
                else Assert.AreEqual(0, lava, room.id + " should have no lava");
                for (int y = 0; y < room.height; y++)
                    for (int x = 0; x < room.width; x++)
                        if (room.cells[x, y] == Glyphs.Lava)
                            Assert.IsTrue(room.IsSolid(x, y - 1) || room.Get(x, y - 1) == Glyphs.Lava, $"{room.id}: lava at ({x},{y}) has no floor");
            }
        }

        [Test]
        public void TallTowers_AndThroneRoom()
        {
            var rooms = Rooms(out _, out _);
            Assert.AreEqual(44, rooms["P2_D2"].height, "D2 is 40×44");
            Assert.AreEqual(66, rooms["P2_D4"].height, "D4 is 40×66");
            RoomData throne = rooms["P2_THRONE"];
            Assert.AreEqual(1, Count(throne, Glyphs.FinalBoss));
            Assert.AreEqual(1, Count(throne, Glyphs.Gate), "one gate at the entrance");
            Assert.AreEqual(0, Count(throne, Glyphs.Spikes), "no ceiling hazards");
            Assert.AreEqual(0, throne.CountEnemies());
            Assert.Greater(Count(rooms["P2_D8"], Glyphs.Bench), 0, "bench in the room before the throne");
            Assert.AreEqual(2, Count(rooms["P2_MB"], Glyphs.Gate), "Colossus arena: entry + exit gate now that Section D exists");
        }
    }
}
