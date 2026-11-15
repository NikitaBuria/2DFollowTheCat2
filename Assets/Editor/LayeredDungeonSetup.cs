using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class LayeredDungeonSetup
{
    const string Art = "Assets/Art/Map/Dungeon/LayeredShrine/";
    const string Prefab = "Assets/Prefabs/DungeonRoom.prefab";
    const string SceneFile = "Assets/Scenes/DungeonRoom.unity";
    [Serializable] class Prop { public string name; public float x,y; }
    [Serializable] class Boundary { public string name; public Vector2[] points; }
    [Serializable] class Zone : Prop { public float radius; public string light; }
    [Serializable] class Layout { public int width,height,ppu; public Prop[] props; public Boundary[] paths; public Zone[] zones; }
    static Layout map;
    static Vector2 World(float x,float y) => new Vector2(x-map.width*.5f,map.height*.5f-y)/map.ppu;

    static GameObject Child(string name,Transform parent)
    {
        var obj=new GameObject(name);
        obj.transform.SetParent(parent,false);
        return obj;
    }

    static Sprite LoadSprite(string name,bool feet=false)
    {
        string path=Art+name+".png";
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType=TextureImporterType.Sprite;
        importer.spriteImportMode=SpriteImportMode.Single;
        importer.spritePixelsPerUnit=2 * map.ppu;
        importer.filterMode=FilterMode.Bilinear;
        importer.maxTextureSize=4096;
        importer.mipmapEnabled=false;
        importer.alphaIsTransparency=true;
        importer.textureCompression=TextureImporterCompression.Uncompressed;
        var settings=new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteAlignment=(int)(feet?SpriteAlignment.BottomCenter:SpriteAlignment.Center);
        settings.spriteMeshType=SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static void Draw(GameObject obj,Sprite sprite,string layer,int order)
    {
        var renderer=obj.AddComponent<SpriteRenderer>();
        var materialPath=sprite.name == "Map_Base" ? Art+"ShrinePortal.mat" : AssetDatabase.GUIDToAssetPath("a97c105638bdf8b4a8650670310a4cd3");
        renderer.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        renderer.sprite=sprite;
        renderer.sortingLayerName=layer;
        renderer.sortingOrder=order;
        renderer.spriteSortPoint=SpriteSortPoint.Pivot;
    }

    static void LightLayers(Light2D light)
    {
        var serialized=new SerializedObject(light);
        var layers=serialized.FindProperty("m_ApplyToSortingLayers");
        layers.arraySize=2;
        layers.GetArrayElementAtIndex(0).intValue=0;
        layers.GetArrayElementAtIndex(1).intValue=SortingLayer.NameToID("Entities");
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    [MenuItem("Tools/Dungeon/Build Layered Shrine Room")]
    public static void Build()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        AssetDatabase.Refresh();
        map=JsonUtility.FromJson<Layout>(File.ReadAllText(Art+"Layout.json"));
        Selection.objects=Array.Empty<UnityEngine.Object>();
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var root=new GameObject("DungeonRoom");
        Draw(Child("BaseFloor",root.transform),LoadSprite("Map_Base"),"Default",-10);
        LoadSprite("Map_Occluders");
        var front=Child("ForegroundOccluders",root.transform);
        foreach(var prop in map.props)
        {
            var obj=Child(prop.name,front.transform);
            obj.transform.localPosition=World(prop.x,prop.y);
            Draw(obj,LoadSprite("Props/"+prop.name,true),"Default",0);
        }
        Draw(Child("TopOverhangs",root.transform),LoadSprite("Map_Overhangs"),"Default",50);
        var edges=Child("WalkableColliders",root.transform);
        foreach(var path in map.paths)
            Child(path.name,edges.transform).AddComponent<EdgeCollider2D>().points=path.points.Select(p=>World(p.x,p.y)).ToArray();
        var triggers=Child("Triggers",root.transform);
        foreach(var zone in map.zones)
        {
            var obj=Child(zone.name,triggers.transform);
            obj.transform.localPosition=World(zone.x,zone.y);
            var circle=obj.AddComponent<CircleCollider2D>();
            circle.isTrigger=true;
            circle.radius=zone.radius/map.ppu;
            if(string.IsNullOrEmpty(zone.light)) continue;
            var lamp=obj.AddComponent<Light2D>();
            lamp.lightType=Light2D.LightType.Point;
            lamp.color=zone.light=="green"?new Color(.25f,1,.38f):new Color(1,.46f,.12f);
            lamp.pointLightOuterRadius=2;
            lamp.pointLightInnerRadius=.3f;
            lamp.intensity=.55f;
            LightLayers(lamp);
        }
        var spawn=Child("PortalArrival",root.transform);
        spawn.transform.localPosition=World(160,922);
        var ambient=new GameObject("AmbientLight").AddComponent<Light2D>();
        ambient.lightType=Light2D.LightType.Global;
        ambient.color=Color.white;
        ambient.intensity=.85f;
        LightLayers(ambient);
        PrefabUtility.SaveAsPrefabAssetAndConnect(root,Prefab,InteractionMode.AutomatedAction);
        var core=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Core.prefab"));
        var roomCamera=core.GetComponentInChildren<Unity.Cinemachine.CinemachineCamera>(true);
        var lens=roomCamera.Lens;
        lens.OrthographicSize=8.5f;
        roomCamera.Lens=lens;
        PrefabUtility.RecordPrefabInstancePropertyModifications(roomCamera);
        var player=core.GetComponentInChildren<Player>(true);
        player.transform.position=spawn.transform.position;
        PrefabUtility.RecordPrefabInstancePropertyModifications(player.transform);
        var group=player.GetComponent<SortingGroup>();
        if(!group) group=player.gameObject.AddComponent<SortingGroup>();
        group.sortingLayerName="Default";
        group.sortingOrder=0;
        var respawn=new SerializedObject(core.GetComponentInChildren<RespawnManager>(true));
        respawn.FindProperty("spawnPoint").objectReferenceValue=spawn.transform;
        respawn.ApplyModifiedPropertiesWithoutUndo();
        foreach(var lamp in core.GetComponentsInChildren<Light2D>(true)) LightLayers(lamp);
        foreach(var camera in core.GetComponentsInChildren<Camera>(true))
        {
            camera.transparencySortMode=TransparencySortMode.CustomAxis;
            camera.transparencySortAxis=Vector3.up;
        }
        EditorSceneManager.SaveScene(scene,SceneFile);
        AssetDatabase.SaveAssets();
        Debug.Log("Built layered dungeon at "+SceneFile);
    }
}
