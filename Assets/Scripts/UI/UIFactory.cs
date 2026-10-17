using System;
using Shatranj.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Shatranj.UI
{
    /// <summary>
    /// Runtime uGUI builders shared by the cutscene caption box, main menu, pause menu and screen cards: 1920×1080
    /// overlay canvases, panels, TextMeshPro labels, cut-paper buttons (Button_Normal / Button_Hover placeholder art,
    /// sprite-swap) and an EventSystem with the Input System UI module (keyboard, gamepad and mouse navigation).
    /// </summary>
    public static class UIFactory
    {
        public static readonly Color Ink = new Color(0.13f, 0.11f, 0.10f);
        public static readonly Color Paper = new Color(0.91f, 0.85f, 0.71f);

        public static RectTransform Canvas(string name, Transform parent, int sortingOrder)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            if (parent != null) go.transform.SetParent(parent, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return (RectTransform)go.transform;
        }

        public static RectTransform Rect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var r = (RectTransform)go.transform;
            r.SetParent(parent, false);
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.pivot = pivot;
            r.anchoredPosition = pos;
            r.sizeDelta = size;
            return r;
        }

        public static RectTransform Stretch(string name, Transform parent) => Rect(name, parent, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

        public static Image Image(string name, Transform parent, Sprite sprite, Color color, bool stretch = true)
        {
            RectTransform r = stretch ? Stretch(name, parent) : Rect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(100f, 100f));
            var img = r.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        public static TextMeshProUGUI Text(string name, Transform parent, string content, float size, TextAlignmentOptions align, Color color)
        {
            RectTransform r = Stretch(name, parent);
            var t = r.gameObject.AddComponent<TextMeshProUGUI>();
            t.text = content;
            t.fontSize = size;
            t.alignment = align;
            t.color = color;
            t.raycastTarget = false;
            t.textWrappingMode = TextWrappingModes.Normal;
            return t;
        }

        public static Sprite CatalogSprite(string name)
        {
            SpriteCatalog cat = SpriteCatalog.Instance;
            return cat != null ? cat.Find(name) : null;
        }

        /// <summary>Cut-paper menu button (360×80 art), sprite swap on hover/selection.</summary>
        public static Button Button(string name, Transform parent, string label, Vector2 pos, Action onClick)
        {
            RectTransform r = Rect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, new Vector2(360f, 80f));
            var img = r.gameObject.AddComponent<Image>();
            img.sprite = CatalogSprite("Button_Normal");
            img.color = img.sprite != null ? Color.white : Paper;
            var button = r.gameObject.AddComponent<Button>();
            Sprite hover = CatalogSprite("Button_Hover");
            if (hover != null)
            {
                button.transition = Selectable.Transition.SpriteSwap;
                button.spriteState = new SpriteState { highlightedSprite = hover, selectedSprite = hover, pressedSprite = hover };
            }
            else
            {
                ColorBlock cb = button.colors;
                cb.highlightedColor = new Color(0.8f, 0.7f, 0.55f);
                cb.selectedColor = cb.highlightedColor;
                button.colors = cb;
            }
            button.onClick.AddListener(() => onClick?.Invoke());
            TextMeshProUGUI t = Text("Label", r, label, 34, TextAlignmentOptions.Center, Ink);
            t.fontStyle = FontStyles.Bold;
            return button;
        }

        /// <summary>Links buttons top-to-bottom (wrapping) for keyboard / gamepad navigation.</summary>
        public static void ChainVertical(params Button[] buttons)
        {
            var active = new System.Collections.Generic.List<Button>();
            foreach (Button b in buttons) if (b != null && b.interactable && b.gameObject.activeSelf) active.Add(b);
            for (int i = 0; i < active.Count; i++)
            {
                var nav = new Navigation
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = active[(i - 1 + active.Count) % active.Count],
                    selectOnDown = active[(i + 1) % active.Count],
                };
                active[i].navigation = nav;
            }
        }

        /// <summary>Creates an EventSystem with the Input System UI module if the scene has none.</summary>
        public static EventSystem EnsureEventSystem()
        {
            if (EventSystem.current != null) return EventSystem.current;
            EventSystem existing = UnityEngine.Object.FindFirstObjectByType<EventSystem>();
            if (existing != null) return existing;
            var go = new GameObject("EventSystem", typeof(EventSystem));
            var module = go.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
            return go.GetComponent<EventSystem>();
        }
    }
}
