using System;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class LootVisualBuilder
{
    public static void Build()
    {
        const string art="Assets/Art/Items/Loot/GeneratedPickups";
        const string prefabs="Assets/Prefabs/Loot";
        var sprites=new Sprite[6];
        for(int i=0;i<7;i++)
        {
            string path=art+"/"+(i==6?"WispEssence":"GoldCoin_"+i)+".png";
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var imp=(TextureImporter)AssetImporter.GetAtPath(path);
            imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Single;
            imp.spritePixelsPerUnit=64;imp.filterMode=FilterMode.Point;
            imp.textureCompression=TextureImporterCompression.Uncompressed;imp.mipmapEnabled=false;imp.alphaIsTransparency=true;
            imp.SaveAndReimport();if(i<6)sprites[i]=AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        var essence=AssetDatabase.LoadAssetAtPath<Sprite>(art+"/WispEssence.png");
        var item=AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Scripts/ScriptableObjects/Items/WispEssence.asset");
        if(item==null)throw new Exception("Wisp Essence item data missing.");
        var data=new SerializedObject(item);data.FindProperty("<icon>k__BackingField").objectReferenceValue=essence;data.ApplyModifiedPropertiesWithoutUndo();
        var clip=new AnimationClip{name="GoldCoin_Spin",frameRate=8};
        var keys=new ObjectReferenceKeyframe[6];for(int i=0;i<6;i++)keys[i]=new ObjectReferenceKeyframe{time=i/8f,value=sprites[i]};
        AnimationUtility.SetObjectReferenceCurve(clip,EditorCurveBinding.PPtrCurve("",typeof(SpriteRenderer),"m_Sprite"),keys);
        var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=true;AnimationUtility.SetAnimationClipSettings(clip,settings);
        if(File.Exists(art+"/GoldCoin.controller"))throw new Exception("Generated pickup assets already exist.");
        AssetDatabase.CreateAsset(clip,art+"/GoldCoin_Spin.anim");
        var controller=AnimatorController.CreateAnimatorControllerAtPath(art+"/GoldCoin.controller");
        var state=controller.layers[0].stateMachine.AddState("Spin");state.motion=clip;controller.layers[0].stateMachine.defaultState=state;
        for(int n=0;n<2;n++)
        {
            string output=prefabs+"/"+(n==0?"GoldCoinPickup":"WispEssencePickup")+".prefab";
            if(File.Exists(output))throw new Exception("Pickup prefab already exists: "+output);
            var go=PrefabUtility.LoadPrefabContents(prefabs+"/"+(n==0?"CoinPickup":"ItemPickup")+".prefab");
            try
            {
                go.name=n==0?"GoldCoinPickup":"WispEssencePickup";
                var renderer=go.GetComponent<SpriteRenderer>();renderer.sprite=n==0?sprites[0]:essence;renderer.sortingOrder=3;
                if(n==0)
                {
                    var animator=go.GetComponent<Animator>();if(animator==null)animator=go.AddComponent<Animator>();animator.runtimeAnimatorController=controller;
                    foreach(var key in keys){clip.SampleAnimation(go,key.time+.001f);if(renderer.sprite!=key.value)throw new Exception("Coin animation frame mismatch");}
                    renderer.sprite=sprites[0];
                }
                else
                {
                    var pickup=go.GetComponent<ItemPickup>();var serialized=new SerializedObject(pickup);
                    serialized.FindProperty("item").objectReferenceValue=item;
                    serialized.FindProperty("amount").intValue=1;
                    serialized.FindProperty("spriteRenderer").objectReferenceValue=renderer;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                    pickup.Configure(item,1);
                    if(renderer.sprite!=essence)throw new Exception("Essence Configure replaced icon incorrectly.");
                }
                if(go.GetComponent<Collider2D>()==null||!go.GetComponent<Collider2D>().isTrigger)throw new Exception("Missing pickup trigger");
                PrefabUtility.SaveAsPrefabAsset(go,output);
            }
            finally{PrefabUtility.UnloadPrefabContents(go);}
            var saved=AssetDatabase.LoadAssetAtPath<GameObject>(output);
            if(saved.GetComponent<Pickup>()==null||saved.GetComponent<SpriteRenderer>().sprite==null)throw new Exception("Invalid saved pickup.");
        }
        AssetDatabase.SaveAssets();
        Debug.Log("PASS: GoldCoinPickup and WispEssencePickup saved with existing pickup components and trigger colliders; six coin frames and essence item icon/Configure verified.");
    }
}

