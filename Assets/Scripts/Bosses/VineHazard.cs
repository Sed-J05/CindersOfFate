using System.Collections;
using Shatranj.Core;
using Shatranj.Data;
using Shatranj.Visuals;
using UnityEngine;

namespace Shatranj.Bosses
{
    /// <summary>
    /// One Zulmat vine (Docs/04 §E Vine Summon, Docs/06 §4 Vine 128×384, pivot bottom): erupts from the floor with its
    /// Grow clip, hurts on touch (1.0 × 3.0 u, EnemyAttack trigger + DamageDealer) for <c>stay</c> seconds, then plays
    /// Retract and removes itself. Built at runtime — no prefab.
    /// </summary>
    public class VineHazard : MonoBehaviour
    {
        private const float ArtHeight = 3.84f;   // 384 px at 100 px/u
        private const float RetractTime = 3f / 16f;

        private BoxCollider2D box;
        private SpriteAnimator anim;
        private float stay;

        public bool Finished { get; private set; }

        public static VineHazard Spawn(Vector2 feet, Vector2 size, float stay, int damage, GameObject owner, Transform parent)
        {
            var go = new GameObject("Vine");
            go.layer = GameLayers.EnemyAttack;
            go.transform.SetParent(parent, true);
            go.transform.position = feet;

            var visual = new GameObject("Visual");
            visual.layer = GameLayers.Ignore;
            visual.transform.SetParent(go.transform, false);
            float s = size.y / ArtHeight;
            visual.transform.localScale = new Vector3(s, s, 1f);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = GameLayers.Sorting.Enemies;
            sr.sortingOrder = 20;
            var anim = visual.AddComponent<SpriteAnimator>();
            anim.Set = SpriteAnimationSet.Load("Vine");

            var box = go.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = size;
            box.offset = new Vector2(0f, size.y * 0.5f);
            go.AddComponent<DamageDealer>().Configure(damage, DamageType.Melee, owner);

            var vine = go.AddComponent<VineHazard>();
            vine.box = box;
            vine.anim = anim;
            vine.stay = stay;
            return vine;
        }

        private void Start() => StartCoroutine(Life());

        private IEnumerator Life()
        {
            if (anim != null) anim.Play("Grow", restart: true);
            AudioManager.Play("zulmat_vine");
            Feel.Play(FeelEvent.LightImpact);
            float t = 0f;
            while (t < stay) { t += Time.deltaTime; yield return null; }
            box.enabled = false;
            if (anim != null) anim.Play("Retract", restart: true);
            t = 0f;
            while (t < RetractTime) { t += Time.deltaTime; yield return null; }
            Finished = true;
            Destroy(gameObject);
        }
    }
}
