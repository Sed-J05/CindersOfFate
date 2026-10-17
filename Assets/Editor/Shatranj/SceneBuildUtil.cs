using System.IO;
using Shatranj.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Shatranj.EditorTools
{
    /// <summary>
    /// Shared helpers for scene-building menu items: placeholder sprite, materials, boxes, camera, light,
    /// debug canvas, Build Settings registration. Everything is idempotent and never overwrites existing art.
    /// </summary>
    public static class SceneBuildUtil
    {
        public const string PlaceholderFolder = "Assets/Art/Placeholders";
        public const string WhiteSpritePath = PlaceholderFolder + "/Square_White.png";
        public const string PhysicsFolder = "Assets/Data/Physics";
        public const string ZeroFrictionPath = PhysicsFolder + "/ZeroFriction.physicsMaterial2D";
        public const string SpriteLitMaterialPath = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Lit-Default.mat";
        public const string SpriteUnlitMaterialPath = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";

        /// <summary>32×32 white square at 32 PPU, so one sprite unit = one world unit. Created once, never overwritten.</summary>
        public static Sprite GetOrCreateWhiteSprite()
        {
            if (!File.Exists(WhiteSpritePath))
            {
                GenerateConfigs.EnsureFolder(PlaceholderFolder);
                var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
                var pixels = new Color32[32 * 32];
                for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(255, 255, 255, 255);
                tex.SetPixels32(pixels);
                tex.Apply();
                File.WriteAllBytes(WhiteSpritePath, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(WhiteSpritePath, ImportAssetOptions.ForceSynchronousImport);
            }

            var importer = AssetImporter.GetAtPath(WhiteSpritePath) as TextureImporter;
            if (importer != null && (importer.textureType != TextureImporterType.Sprite || importer.spritePixelsPerUnit != 32))
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 32;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                settings.spritePivot = new Vector2(0.5f, 0.5f);
                importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(WhiteSpritePath);
        }

        public static Material SpriteLitMaterial()
        {
            return AssetDatabase.LoadAssetAtPath<Material>(SpriteLitMaterialPath);
        }

        public static Material SpriteUnlitMaterial()
        {
            return AssetDatabase.LoadAssetAtPath<Material>(SpriteUnlitMaterialPath);
        }

        public static PhysicsMaterial2D GetOrCreateZeroFrictionMaterial()
        {
            var mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(ZeroFrictionPath);
            if (mat != null) return mat;
            GenerateConfigs.EnsureFolder(PhysicsFolder);
            mat = new PhysicsMaterial2D("ZeroFriction") { friction = 0f, bounciness = 0f };
            AssetDatabase.CreateAsset(mat, ZeroFrictionPath);
            return mat;
        }

        /// <summary>A solid or one-way box: sprite + collider sized by transform scale.</summary>
        public static GameObject CreateBox(string name, Transform parent, Vector2 center, Vector2 size, Color color, int layer, string sortingLayer, bool oneWay = false)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(center.x, center.y, 0f);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            go.layer = layer;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetOrCreateWhiteSprite();
            sr.color = color;
            sr.sortingLayerName = sortingLayer;
            Material lit = SpriteLitMaterial();
            if (lit != null) sr.sharedMaterial = lit;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;

            if (oneWay)
            {
                var effector = go.AddComponent<PlatformEffector2D>();
                effector.useOneWay = true;
                effector.useOneWayGrouping = false;
                effector.surfaceArc = 170f;
                effector.useSideFriction = false;
                effector.useSideBounce = false;
                col.usedByEffector = true;
            }
            return go;
        }

        public static Camera CreateMainCamera(Vector3 position, float orthoSize, Color background)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            go.transform.position = position;
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = orthoSize;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = background;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;
            go.AddComponent<AudioListener>();
            UniversalAdditionalCameraData data = cam.GetUniversalAdditionalCameraData();
            data.renderType = CameraRenderType.Base;
            return cam;
        }

        public static Light2D CreateGlobalLight(string name, Color color, float intensity)
        {
            var go = new GameObject(name);
            var light = go.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.color = color;
            light.intensity = intensity;
            return light;
        }

        public static Font BuiltinFont()
        {
            Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return f;
        }

        /// <summary>Screen-space canvas with a top-left legacy Text block for debug readouts.</summary>
        public static Text CreateDebugTextCanvas(string name, out Canvas canvas)
        {
            var canvasGo = new GameObject(name, typeof(RectTransform));
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var textGo = new GameObject("DebugText", typeof(RectTransform));
            textGo.transform.SetParent(canvasGo.transform, false);
            var rect = (RectTransform)textGo.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(16f, -16f);
            rect.sizeDelta = new Vector2(1000f, 320f);

            var text = textGo.AddComponent<Text>();
            text.font = BuiltinFont();
            text.fontSize = 22;
            text.color = new Color(0.1f, 0.1f, 0.12f);
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.text = "debug";
            return text;
        }

        public static void AddSceneToBuildSettings(string scenePath)
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            foreach (EditorBuildSettingsScene s in scenes)
                if (s.path == scenePath) { return; }

            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(scenes)
            {
                new EditorBuildSettingsScene(scenePath, true),
            };
            EditorBuildSettings.scenes = list.ToArray();
        }

        /// <summary>Sandbox training dummy: Enemy-layer root, EnemyHurtbox trigger child, Health + DamageableDummy.</summary>
        public static GameObject CreateDummy(string name, Vector2 feetPosition, int hp, Vector2 bodySize, Color bodyColor, Color accentColor)
        {
            var root = new GameObject(name);
            root.layer = GameLayers.Enemy;
            root.tag = GameLayers.Tags.Enemy;
            root.transform.position = feetPosition;

            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = new Vector3(0f, bodySize.y * 0.5f, 0f);
            visual.transform.localScale = new Vector3(bodySize.x, bodySize.y, 1f);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = GetOrCreateWhiteSprite();
            sr.color = bodyColor;
            sr.sortingLayerName = GameLayers.Sorting.Enemies;
            Material lit = SpriteLitMaterial();
            if (lit != null) sr.sharedMaterial = lit;

            var eye = new GameObject("Eye");
            eye.transform.SetParent(visual.transform, false);
            eye.transform.localPosition = new Vector3(-0.25f, 0.25f, 0f);
            eye.transform.localScale = new Vector3(0.18f, 0.1f, 1f);
            var eyeSr = eye.AddComponent<SpriteRenderer>();
            eyeSr.sprite = GetOrCreateWhiteSprite();
            eyeSr.color = accentColor;
            eyeSr.sortingLayerName = GameLayers.Sorting.Enemies;
            eyeSr.sortingOrder = 1;
            if (lit != null) eyeSr.sharedMaterial = lit;

            var hurtboxGo = new GameObject("Hurtbox");
            hurtboxGo.transform.SetParent(root.transform, false);
            hurtboxGo.layer = GameLayers.EnemyHurtbox;
            var hurtbox = hurtboxGo.AddComponent<BoxCollider2D>();
            hurtbox.isTrigger = true;
            hurtbox.size = new Vector2(bodySize.x * 0.9f, bodySize.y * 0.9f);
            hurtbox.offset = new Vector2(0f, bodySize.y * 0.5f);

            var health = root.AddComponent<Health>();
            SetInt(health, "maxHp", hp);
            SetFloat(health, "invulnerabilityDuration", 0f);
            SetFloat(health, "hitFlashDuration", 0.08f);

            var dummy = root.AddComponent<Shatranj.Enemies.DamageableDummy>();
            SetReference(dummy, "body", sr);
            SetReference(dummy, "hurtbox", hurtbox);
            return root;
        }

        /// <summary>Spike strip: Hazard-layer trigger with a DamageDealer (type Hazard → safe-ground respawn).</summary>
        public static GameObject CreateSpikes(string name, Transform parent, Vector2 center, Vector2 size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(center.x, center.y, 0f);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            go.layer = GameLayers.Hazard;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetOrCreateWhiteSprite();
            sr.color = color;
            sr.sortingLayerName = GameLayers.Sorting.Props;
            Material lit = SpriteLitMaterial();
            if (lit != null) sr.sharedMaterial = lit;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;
            col.isTrigger = true;

            var dealer = go.AddComponent<DamageDealer>();
            SetInt(dealer, "damage", 1);
            SetEnum(dealer, "type", (int)DamageType.Hazard);
            return go;
        }

        public static void SetInt(Object target, string field, int value)
        {
            var so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(field);
            if (p == null) { Debug.LogWarning($"SceneBuildUtil: field '{field}' not found on {target.GetType().Name}"); return; }
            p.intValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetFloat(Object target, string field, float value)
        {
            var so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(field);
            if (p == null) { Debug.LogWarning($"SceneBuildUtil: field '{field}' not found on {target.GetType().Name}"); return; }
            p.floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetObjectArray(Object target, string field, Object[] values)
        {
            var so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(field);
            if (p == null || !p.isArray) { Debug.LogWarning($"SceneBuildUtil: array '{field}' not found on {target.GetType().Name}"); return; }
            p.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetString(Object target, string field, string value)
        {
            var so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(field);
            if (p == null) { Debug.LogWarning($"SceneBuildUtil: field '{field}' not found on {target.GetType().Name}"); return; }
            p.stringValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetEnum(Object target, string field, int enumValue)
        {
            var so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(field);
            if (p == null) { Debug.LogWarning($"SceneBuildUtil: field '{field}' not found on {target.GetType().Name}"); return; }
            p.enumValueIndex = enumValue;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Assigns a serialized (possibly private) object reference field on a component.</summary>
        public static void SetReference(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(field);
            if (p == null) { Debug.LogWarning($"SceneBuildUtil: field '{field}' not found on {target.GetType().Name}"); return; }
            if (value == null)
            {
                // Catches Unity "fake null": a C# reference to an asset that was unloaded (e.g. by NewScene).
                Debug.LogWarning($"SceneBuildUtil: value for '{field}' on {target.GetType().Name} is null or destroyed — reference not set.");
                return;
            }
            p.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            if (p.objectReferenceValue == null)
                Debug.LogWarning($"SceneBuildUtil: '{field}' on {target.GetType().Name} rejected {value.name} ({value.GetType().Name}).");
        }
    }
}
