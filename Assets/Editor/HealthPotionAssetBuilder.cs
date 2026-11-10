using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class HealthPotionAssetBuilder
{
    private const string Art = "Assets/Art/Items/Consumables/HealthPotion.png";
    private const string Data = "Assets/Scripts/ScriptableObjects/Items/HealthPotion.asset";
    private const string Prefab = "Assets/Prefabs/Loot/HealthPotionPickup.prefab";

    [InitializeOnLoadMethod]
    private static void QueueBuild()
    {
        EditorApplication.delayCall += () =>
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode && File.Exists(Art) && !File.Exists("Assets/Art/Items/Consumables/Validation.txt")) Build();
        };
    }

    [MenuItem("Tools/Items/Build Health Potion")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (File.Exists(Prefab))
        {
            var existing = PrefabUtility.LoadPrefabContents(Prefab);
            try
            {
                Verify(AssetDatabase.LoadAssetAtPath<HealthPotionData>(Data), existing.GetComponent<ItemPickup>());
                File.WriteAllText("Assets/Art/Items/Consumables/Validation.txt", "PASS: pickup, full-health preservation, healing, consumption, maximum-health cap, dead-user rejection. Editor-only checks; hotbar input wiring is left to the developer.");
            }
            finally { PrefabUtility.UnloadPrefabContents(existing); }
            return;
        }
        AssetDatabase.ImportAsset(Art, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(Art);
        importer.GetSourceTextureWidthAndHeight(out int width, out int height);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = Mathf.Max(width, height) * 2f;
        importer.maxTextureSize = 128;
        importer.mipmapEnabled = false;
        importer.isReadable = false;
        importer.alphaIsTransparency = true;
        importer.filterMode = FilterMode.Point;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.textureCompression = TextureImporterCompression.CompressedHQ;
        importer.SaveAndReimport();
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art);
        var item = AssetDatabase.LoadAssetAtPath<HealthPotionData>(Data);
        if (item == null)
        {
            item = ScriptableObject.CreateInstance<HealthPotionData>();
            var serialized = new SerializedObject(item);
            serialized.FindProperty("<itemName>k__BackingField").stringValue = "Health Potion";
            serialized.FindProperty("<icon>k__BackingField").objectReferenceValue = sprite;
            serialized.FindProperty("<maxStack>k__BackingField").intValue = 10;
            serialized.FindProperty("<sellPrice>k__BackingField").intValue = 5;
            serialized.FindProperty("<healAmount>k__BackingField").intValue = 3;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(item, Data);
        }
        var go = new GameObject("HealthPotionPickup");
        try
        {
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 3;
            var collider = go.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.2f;
            var pickup = go.AddComponent<ItemPickup>();
            var serialized = new SerializedObject(pickup);
            serialized.FindProperty("spriteRenderer").objectReferenceValue = renderer;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            pickup.Configure(item, 1);
            Verify(item, pickup);
            // The pickup test consumes its quantity, so restore the prefab's single potion.
            pickup.Configure(item, 1);
            PrefabUtility.SaveAsPrefabAsset(go, Prefab);
            AssetDatabase.SaveAssets();
            Debug.Log("PASS Health Potion: pickup adds item; full health preserves stack; injured use heals and consumes one; healing caps at maximum; dead users cannot heal.");
        }
        finally { UnityEngine.Object.DestroyImmediate(go); }
    }

    private static void Verify(HealthPotionData item, ItemPickup pickup)
    {
        var user = new GameObject("PotionValidation", typeof(Health), typeof(Inventory));
        try
        {
            var health = user.GetComponent<Health>();
            var inventory = user.GetComponent<Inventory>();
            typeof(Inventory).GetMethod("EnsureSlots", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(inventory, null);
            health.Initialize(10);
            if (!pickup.TryPickUp(user) || !inventory.HasItem(item)) throw new Exception("Potion pickup failed.");
            if (inventory.UseItem(0, user) || !inventory.HasItem(item)) throw new Exception("Full health consumed a potion.");
            // Set a known injured state without invoking combat or player state transitions.
            typeof(Health).GetProperty("currentHealth").SetValue(health, 5);
            if (!inventory.UseItem(0, user) || health.currentHealth != 8 || inventory.HasItem(item)) throw new Exception("Healing/consumption failed.");
            if (!item.Use(user) || health.currentHealth != 10) throw new Exception("Maximum health cap failed.");
            typeof(Health).GetProperty("currentHealth").SetValue(health, 0);
            if (item.Use(user)) throw new Exception("Potion revived a dead user.");
        }
        finally { UnityEngine.Object.DestroyImmediate(user); }
    }
}
