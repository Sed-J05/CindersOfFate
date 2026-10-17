using NUnit.Framework;
using Shatranj.Data;
using UnityEngine;

namespace Shatranj.Tests
{
    /// <summary>Docs/07 §3–4 flame arc and enemy-scale curve (Docs/02 §13 asks for these tests).</summary>
    public class FlameMathTests
    {
        private const float Eps = 1e-4f;

        [Test]
        public void Arc_StartsBottomLeft_PeaksCentre_EndsBottomRight()
        {
            Vector2 start = FlameMath.Arc(0f, 0.05f, 0.95f, 0.05f, 0.85f);
            Vector2 mid = FlameMath.Arc(0.5f, 0.05f, 0.95f, 0.05f, 0.85f);
            Vector2 end = FlameMath.Arc(1f, 0.05f, 0.95f, 0.05f, 0.85f);
            Assert.AreEqual(0.05f, start.x, Eps); Assert.AreEqual(0.05f, start.y, Eps);
            Assert.AreEqual(0.5f, mid.x, Eps); Assert.AreEqual(0.9f, mid.y, Eps);
            Assert.AreEqual(0.95f, end.x, Eps); Assert.AreEqual(0.05f, end.y, Eps);
        }

        [Test]
        public void Arc_ClampsT()
        {
            Assert.AreEqual(FlameMath.Arc(1f, 0.05f, 0.95f, 0.05f, 0.85f), FlameMath.Arc(3f, 0.05f, 0.95f, 0.05f, 0.85f));
            Assert.AreEqual(FlameMath.Arc(0f, 0.05f, 0.95f, 0.05f, 0.85f), FlameMath.Arc(-1f, 0.05f, 0.95f, 0.05f, 0.85f));
        }

        [Test]
        public void Scale_Monotonic_IsOneToMax_EaseInQuad()
        {
            Assert.AreEqual(1f, FlameMath.Scale(0f, FlameScaleMode.Monotonic, 1.4f), Eps);
            Assert.AreEqual(1.4f, FlameMath.Scale(1f, FlameScaleMode.Monotonic, 1.4f), Eps);
            Assert.AreEqual(1.1f, FlameMath.Scale(0.5f, FlameScaleMode.Monotonic, 1.4f), Eps, "lerp(1, 1.4, 0.25)");
            float prev = 0f;
            for (int i = 0; i <= 20; i++)
            {
                float s = FlameMath.Scale(i / 20f, FlameScaleMode.Monotonic, 1.4f);
                Assert.GreaterOrEqual(s, prev);
                prev = s;
            }
        }

        [Test]
        public void Scale_Peak_IsMaxAtMiddle_OneAtEnds()
        {
            Assert.AreEqual(1f, FlameMath.Scale(0f, FlameScaleMode.Peak, 1.4f), Eps);
            Assert.AreEqual(1.4f, FlameMath.Scale(0.5f, FlameScaleMode.Peak, 1.4f), Eps);
            Assert.AreEqual(1f, FlameMath.Scale(1f, FlameScaleMode.Peak, 1.4f), 1e-3f);
        }

        [Test]
        public void PingPong_SweepsThereAndBack()
        {
            Assert.AreEqual(0f, FlameMath.PingPong(0f, 480f), Eps);
            Assert.AreEqual(0.5f, FlameMath.PingPong(240f, 480f), Eps);
            Assert.AreEqual(1f, FlameMath.PingPong(480f, 480f), Eps);
            Assert.AreEqual(0.5f, FlameMath.PingPong(720f, 480f), Eps);
            Assert.AreEqual(0f, FlameMath.PingPong(960f, 480f), Eps);
        }

        [Test]
        public void FlameConfig_Phase1DoesNotScale_Phase2Does()
        {
            var p1 = ScriptableObject.CreateInstance<FlameConfig>();
            Shatranj.EditorTools.ConfigDefaults.FlamePhase1(p1);
            var p2 = ScriptableObject.CreateInstance<FlameConfig>();
            Shatranj.EditorTools.ConfigDefaults.FlamePhase2(p2);
            Assert.AreEqual(1f, p1.EvaluateScale(0.9f), Eps);
            Assert.AreEqual(1.4f, p2.EvaluateScale(1f), Eps);
            Assert.AreEqual(FlameMotion.PingPong, p1.motion);
            Assert.AreEqual(480f, p1.sweepDuration);
            Assert.AreEqual(FlameMotion.OneWay, p2.motion);
            Assert.AreEqual(1200f, p2.sweepDuration);
            Object.DestroyImmediate(p1);
            Object.DestroyImmediate(p2);
        }
    }
}
