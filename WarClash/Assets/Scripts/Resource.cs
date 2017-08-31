﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;
[Serializable]
public struct BundleInfo
{
    public string BundleName;
    public string AssetPath;
}

public enum WaitingType
{
    Asset, Bundle
}
public struct WaitingBundle
{
    public WaitingType WaitingType;
    public string Name;
    public System.Action<string, UnityEngine.Object> Action;
}

class BundleLoader : IPool
{
    public Action<string, AssetBundle> OnLoadFinish;
    public string BundleName;
    protected IEnumerator LoadCoroutine;
    private string[] dependencies;
    private int count;
    private Object[] depenObjs;
    public void Start(string bundleName, Action<string, AssetBundle> onLoadFinish)
    {
        this.BundleName = bundleName;
        LoadCoroutine = LoadMainAsset();
        dependencies = Resource.Manifest.GetAllDependencies(bundleName);
        Resource.AddLoadingLoader(this);
        if (dependencies.Length > 0)
        {
            depenObjs = new Object[dependencies.Length];
            for (int i = dependencies.Length-1; i >= 0; i--)
            {
                Resource.LoadBundle(dependencies[i], OnDependencyLoadFinish);
            }
        }
        else
        {
            Main.SP.StartCoroutine(LoadCoroutine);
        }
    }
    public void OnDependencyLoadFinish(string dPath, Object bundle)
    {
        count ++;
        if(count == dependencies.Length)
        {
            Main.SP.StartCoroutine(LoadCoroutine);
        }
    }
    //System.Diagnostics.Stopwatch sw = new Stopwatch();
    //sw.Start();
    IEnumerator LoadMainAsset()
    {
        var asyn = AssetBundle.LoadFromFileAsync(Resource.BaseUrl+BundleName);
        yield return asyn.assetBundle;
        var assetsReq = asyn.assetBundle.LoadAllAssetsAsync();
        yield return assetsReq;
        Resource.RemoveFinishedLoader(this, BundleName, assetsReq.allAssets, asyn.assetBundle);
        Pool.SP.Recycle(this);
    }
    public void Reset()
    {
        dependencies = null;
        count = 0;
        depenObjs = null;
        BundleName = null;
        OnLoadFinish = null;
        Main.SP.StopCoroutine(LoadCoroutine);
    }
}


class Resource
{
    public static string BaseUrl;
    public static AssetBundleManifest Manifest;
    public static Dictionary<string, BundleInfo> AssetsInfos; 
    private static readonly List<BundleLoader> LoadingList = new List<BundleLoader>();
    private static readonly Dictionary<string,Dictionary<string, Object>> BundleNameAssets = new Dictionary<string, Dictionary<string, Object>>(); 
    private static readonly Dictionary<string, AssetBundle> LoadedBundles = new Dictionary<string, AssetBundle>(); 
    private static readonly List<WaitingBundle> WaitingList = new List<WaitingBundle>();
    public static void UnloadBundles()
    {
        foreach (var loadedBundle in LoadedBundles)
        {
            loadedBundle.Value.Unload(false);
        }
        LoadedBundles.Clear();
    }
    public static void RemoveFinishedLoader(BundleLoader l, string bundleName, Object[] assets, AssetBundle bundle)
    {
        string[] assetsNames = bundle.GetAllAssetNames();
        if (!BundleNameAssets.ContainsKey(bundleName))
        {
            BundleNameAssets[bundleName] = new Dictionary<string, Object>();
            for (int i = 0; i < assetsNames.Length; i++)
            {
                BundleNameAssets[bundleName][assetsNames[i]] = assets[i];
            }
            LoadedBundles[bundleName] = bundle;
        }
        LoadingList.Remove(l);
        for (int i = 0; i < WaitingList.Count; i++)
        {
            var w = WaitingList[i];
            string waitingBundleName = string.Empty;
            if (w.WaitingType == WaitingType.Asset)
            {
                var bundleInfo = GetBundleInfo(w.Name);
                if (bundleInfo.BundleName.Equals(bundleName))
                {
                    WaitingList.RemoveAt(i);
                    i--;
                    w.Action.Invoke(w.Name,
                        BundleNameAssets[bundleInfo.BundleName][bundleInfo.AssetPath]);
                }
            }
            else
            {
                if (w.Name.Equals(bundleName))
                {
                    WaitingList.RemoveAt(i);
                    i--;
                    w.Action.Invoke(w.Name, bundle);
                }
            }
        }
    }
    public static void AddLoadingLoader(BundleLoader l)
    {
        LoadingList.Add(l);
    }
    public static BundleInfo GetBundleInfo(string assetName)
    {
        if (AssetsInfos.ContainsKey(assetName))
        {
            return AssetsInfos[assetName];
        }
        else
        {
            throw new Exception("Asset "+assetName+" Not Exsit");
        }
    }

    public static AssetBundle GetBundleByName(string bundleName)
    {
        if (LoadedBundles.ContainsKey(bundleName))
        {
            return LoadedBundles[bundleName];
        }
        else
        {
            return null;
        }
    }
    private static void LoadManifest()
    {
        BaseUrl = System.IO.Path.Combine(Application.dataPath,@"..\AB\");
        var bundle = AssetBundle.LoadFromFile(BaseUrl + "AB");
        Manifest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        bundle.Unload(false);
        if(Manifest == null)
        {
            throw new System.Exception("Manifest not found ");
        }
        var txt = File.ReadAllBytes(BaseUrl + "assetInfos.txt");
        byte[] decompress = Utility.Decompress(txt);
        var destr = Encoding.UTF8.GetString(decompress);
        AssetsInfos = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, BundleInfo>>(destr);
        if (AssetsInfos == null)
        {
            throw new System.Exception("BundleAssetsDic not found ");
        }
    }
    public static void LoadAsset(string assetName, System.Action<string, UnityEngine.Object> action)
    {
        if (Manifest == null)
        {
            LoadManifest();
        }
        var bundleInfo =  GetBundleInfo(assetName);
        if (BundleNameAssets.ContainsKey(bundleInfo.BundleName))
        {
            action.Invoke(assetName, BundleNameAssets[bundleInfo.BundleName][bundleInfo.AssetPath]);
        }
        else if (IsLoading(assetName, WaitingType.Asset))
        {
            WaitingList.Add(new WaitingBundle() { Action = action, Name = assetName, WaitingType = WaitingType.Asset});
        }
        else
        {
            WaitingList.Add(new WaitingBundle() { Action = action, Name = assetName, WaitingType = WaitingType.Asset});
            var loader = Pool.SP.Get(typeof(BundleLoader)) as BundleLoader;
            loader.Start(bundleInfo.BundleName, null);
        }
    }

    public static void LoadBundle(string bundleName, System.Action<string, UnityEngine.Object> action)
    {
        if (Manifest == null)
        {
            LoadManifest();
        }
        if (LoadedBundles.ContainsKey(bundleName))
        {
            action.Invoke(bundleName, LoadedBundles[bundleName]);
        }
        else if (IsLoading(bundleName, WaitingType.Bundle))
        {
            WaitingList.Add(new WaitingBundle() { Action = action, Name = bundleName, WaitingType = WaitingType.Bundle });
        }
        else
        {
            WaitingList.Add(new WaitingBundle() { Action = action, Name = bundleName, WaitingType = WaitingType.Bundle });
            var loader = Pool.SP.Get(typeof(BundleLoader)) as BundleLoader;
            loader.Start(bundleName, null);
        }
    }
    private static bool IsLoading(string name, WaitingType wt)
    {
        string bundleName = string.Empty;
        if (wt == WaitingType.Asset)
        {
            var bundleInfo = GetBundleInfo(name);
            bundleName = bundleInfo.BundleName;
        }
        else
        {
            bundleName = name;
        }
        for (int i = 0; i < LoadingList.Count; i++)
        {
            if (LoadingList[i].BundleName.Equals(bundleName))
            {
                return true;
            }
        }
        return false;
    }

}