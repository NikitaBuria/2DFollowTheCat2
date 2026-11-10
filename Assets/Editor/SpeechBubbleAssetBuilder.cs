using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

// Visual authoring only. Dialogue decisions and input belong to gameplay systems.
public static class SpeechBubbleAssetBuilder
{
    private const string Root = "Assets/Prefabs/UI/Dialogue";

    [InitializeOnLoadMethod]
    private static void QueueBuild()
    {
        EditorApplication.delayCall += () =>
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode) Build();
        };
    }

    [MenuItem("Tools/Dialogue/Build Speech Bubbles")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        Directory.CreateDirectory(Root);
        AssetDatabase.Refresh();
        Make("CatSpeechBubble", "GUIDE", "Stay close. I'll show you the way.", new Color(0.65f, 0.79f, 0.52f));
        Make("MerchantSpeechBubble", "MERCHANT", "Welcome, traveller. Take a look at my wares.", new Color(0.89f, 0.69f, 0.36f));
        AssetDatabase.SaveAssets();
    }

    private static RectTransform Rect(string name, Transform parent, Vector2 size, Vector2 position)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        return rect;
    }

    private static void Fill(RectTransform rect, Color color)
    {
        var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
        image.color = color;
        image.raycastTarget = false;
    }

    private static void Label(Transform parent, string name, string value, Vector2 size, Vector2 pos, float fontSize, Color color, TMP_FontAsset font)
    {
        var rect = Rect(name, parent, size, pos);
        var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.raycastTarget = false;
        text.richText = false;
    }

    private static void Make(string name, string speaker, string line, Color accent)
    {
        string path = Root + "/" + name + ".prefab";
        if (File.Exists(path)) return;
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (font == null) throw new System.InvalidOperationException("Speech bubbles need the existing LiberationSans SDF font.");
        var root = Rect(name, null, new Vector2(360, 150), Vector2.zero);
        try
        {
            root.pivot = new Vector2(0.5f, 0);
            root.localScale = Vector3.one * 0.005f;
            var canvas = root.gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 100;
            var scaler = root.gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 20;
            var group = root.gameObject.AddComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;

            // Geometric UI keeps the two variants texture-free and crisp at game scale.
            var shadow = Rect("Shadow", root, new Vector2(360, 132), new Vector2(3, 69));
            Fill(shadow, new Color(0, 0, 0, 0.35f));
            var tail = Rect("TailBorder", root, new Vector2(20, 20), new Vector2(0, 14));
            tail.localRotation = Quaternion.Euler(0, 0, 45);
            Fill(tail, accent);
            var panel = Rect("Border", root, new Vector2(360, 132), new Vector2(0, 78));
            Fill(panel, accent);
            var inside = Rect("Background", panel, new Vector2(356, 128), Vector2.zero);
            Fill(inside, new Color(0.09f, 0.105f, 0.10f, 0.98f));
            Label(panel, "SpeakerName", speaker, new Vector2(320, 25), new Vector2(0, 37), 18, accent, font);
            Label(panel, "DialogueText", line, new Vector2(320, 66), new Vector2(0, -13), 23, new Color(0.96f, 0.94f, 0.87f), font);
            Canvas.ForceUpdateCanvases();
            foreach (var text in root.GetComponentsInChildren<TextMeshProUGUI>())
            {
                text.ForceMeshUpdate();
                if (text.isTextOverflowing) throw new System.InvalidOperationException(name + ": default text does not fit.");
            }
            PrefabUtility.SaveAsPrefabAsset(root.gameObject, path);
            Debug.Log("Speech bubble created and default text fit verified: " + path);
        }
        finally { Object.DestroyImmediate(root.gameObject); }
    }
}
