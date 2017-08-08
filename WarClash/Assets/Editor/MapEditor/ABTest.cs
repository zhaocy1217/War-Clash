﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class ABTest : Editor
{
    private const string RequiredUrl = "Assets/RequiredResources/";
    public static Dictionary<Object, int> RefCount = new Dictionary<Object, int>();
    public static List<GameObject> CaculatedGo = new List<GameObject>(); 
    public static List<System.Type> Types = new List<System.Type>() {typeof(RuntimeAnimatorController),typeof(Texture2D), typeof(Mesh), typeof(Material), typeof(Shader), typeof(AnimationClip)};
    [MenuItem("Tools/BuildAssetBundle(Win)")]
    public static void BuildAb()
    {
        var dependencyBundle = new Dictionary<string, string>();
        ClearAbName();
        SetAssetBundleName();
        var path = Path.Combine(Application.dataPath, @"..\AB");
        var manifest = BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.ChunkBasedCompression|BuildAssetBundleOptions.DeterministicAssetBundle, BuildTarget.StandaloneWindows64);
        var bundles = manifest.GetAllAssetBundles();
        for (var i = 0; i < bundles.Length; i++)
        {
            var allAssets = AssetDatabase.GetAssetPathsFromAssetBundle(bundles[i]);
            for (var j = 0; j < allAssets.Length; j++)
            {
                if (allAssets[j].Contains(RequiredUrl))
                {
                    string replacedPath = allAssets[j].Replace(RequiredUrl, "");
                    dependencyBundle[replacedPath] = bundles[i];
                }
            }
        }
        var txt = Newtonsoft.Json.JsonConvert.SerializeObject(dependencyBundle, Formatting.Indented);
        File.WriteAllText(path+@"/bundles.txt", txt);
    }
    [MenuItem("Tools/BuildAssetBundle(Android)")]
    public static void BuildAbForAndroid()
    {
        ClearAbName();
        SetAssetBundleName();
        var path = Path.Combine(Application.streamingAssetsPath, @"AB");
        BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DeterministicAssetBundle, BuildTarget.Android);
    }
    [MenuItem("Tools/SetAssetBundleName")]
    public static void SetAssetBundleName()
    {
        var paths = AssetDatabase.GetAllAssetPaths();
        List<string> filterPaths = new List<string>(paths.Length);
        foreach (var path in paths)
        {
            if(path.Contains(RequiredUrl))
            {
                filterPaths.Add(path);
                var asset = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
                if (asset is GameObject)
                {
                    var go = asset as GameObject;
                    DoGameObject(go, path);
                }
                else if (asset is SceneAsset)
                {
                    var scene = asset as SceneAsset;
                    DoScene(scene, path);
                }
                else
                {
                    DoAsset(asset, path);
                }
            }
        }
    }
    [MenuItem("Tools/ClearAssetBundleName")]
    private static void ClearAbName()
    {
        var paths = AssetDatabase.GetAllAssetPaths();
        foreach (var path in paths)
        {
            if (path.Contains(RequiredUrl))
            {
                var asset = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
                ClearAb(asset);
            }
        }
    }
    static void ClearAb(Object go)
    {
        SetAssetBundleName(string.Empty, go);
        var objs = EditorUtility.CollectDependencies(new Object[] { go });
        for (int i = 0; i < objs.Length; i++)
        {
            var obj = objs[i];
            System.Type t = obj.GetType();
            if (Types.Contains(t) || t.Equals(typeof(SceneAsset)) || t.Equals(typeof(GameObject)))
                SetAssetBundleName(string.Empty, obj);
        }
    }

    public static void SetAssetBundleName(string ap, Object obj)
    {
        var rPath = AssetDatabase.GetAssetPath(obj);
        AssetImporter ai = AssetImporter.GetAtPath(rPath);
        if (ai == null)
        {
        }
        else
        {
            ap = ap.Replace(RequiredUrl, string.Empty);
            ai.assetBundleName = ap;
        }
    }

    static void DoScene(SceneAsset scene, string path)
    {
        SetAssetBundleName(path, scene);
        var objs = EditorUtility.CollectDependencies(new Object[] { scene });
        foreach (Object obj in objs)
        {
            if(obj is GameObject && !CaculatedGo.Contains(obj as GameObject))
            {
                CaculatedGo.Add(obj as GameObject);
                var ap = AssetDatabase.GetAssetPath(obj);
                DoGameObject(obj as GameObject, ap, true);
            }
        }
    }
    
    static void DoGameObject(GameObject go, string path, bool inSceneObj = false)
    {
        if (inSceneObj)
        {
            int count = CheckCount(go);
            if (count > 1)
            { 
                SetAssetBundleName(path, go);
            }
        }
        else
        {
            SetAssetBundleName(path, go);
        }
        CollectDependencies(go, path);
    }
    static void DoAsset(Object go, string path)
    {
        bool done = false;
        Debug.LogError(go.GetType());
        if(go is Texture2D)
        {
            done = DoUISprite(go as Texture);
        }
        if(!done)
        {
            var ap = AssetDatabase.GetAssetPath(go);
            int count = CheckCount(go);
            if (count > 1)
            {
                SetAssetBundleName(ap, go);
            }
        }
    }
    static void CollectDependencies(Object go, string path)
    {
        var objs = EditorUtility.CollectDependencies(new Object[] { go });
        foreach (Object obj in objs)
        {
            if (Types.Contains(obj.GetType()))
            {
                DoAsset(obj, path);
            }
        }
    }
    static bool DoUISprite(Object go)
    {
        var r_path = AssetDatabase.GetAssetPath(go);
        TextureImporter ti = AssetImporter.GetAtPath(r_path) as TextureImporter;
        if(ti!=null && ti.textureType == TextureImporterType.Sprite)
        {
            ti.assetBundleName = ti.spritePackingTag;
            return true;
        }
        return false;
    }
    private static int CheckCount(Object obj)
    {
        if (!RefCount.ContainsKey(obj)) RefCount[obj] = 0;
        return ++RefCount[obj];
    }

}
