﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Logic.LogicObject;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

namespace Logic.LogicObject
{
    class HotFixScene : IScene
    {
        public void Destroy()
        {
            
        }
       
        public void Init()
        {
            StartUpdatePatch();
        }

        public void OnFixedUpdate(long deltaTime)
        {
        }

        public void OnUpdate(float deltaTime)
        {
        }

        private void StartUpdatePatch()
        {
            Main.SP.StartCoroutine(LoadHashFileFromServer());
            Main.SP.StartCoroutine(LoadManifestFile());
            Main.SP.StartCoroutine(LoadAssetInfosFile());
        }
        private List<string> _needDownLoadList = new List<string>();
        IEnumerator LoadAssetInfosFile()
        {
            UnityWebRequest www = UnityWebRequest.Get("http://47.94.204.158/AB/assetInfos.txt");
            yield return www.Send();
            if (www.isError)
            {
                DLog.Log(www.error);
            }
            else
            {
                SaveToPersistentPath("assetInfos.txt", www.downloadHandler.data);
                www.Dispose();
            }
        }
        IEnumerator LoadManifestFile()
        {
            UnityWebRequest www = UnityWebRequest.Get("http://47.94.204.158/AB/AB");
            yield return www.Send();
            if (www.isError)
            {
                DLog.Log(www.error);
            }
            else
            {
                SaveToPersistentPath("AB", www.downloadHandler.data);
                www.Dispose();
            }
        }
        IEnumerator LoadHashFileFromServer()
        {
            UnityWebRequest www = UnityWebRequest.Get("http://47.94.204.158/AB/assetBundleHash.txt");
            yield return www.Send();
            if (www.isError)
            {
                DLog.Log(www.error);
            }
            else
            {
                // Show results as text
                _needDownLoadList.Clear();
                var remoteBundleHash = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(www.downloadHandler.text);
                var localBundleHash = AssetResources.LoadBundleHash();
                foreach (var bh in remoteBundleHash)
                {
                    string hash;
                    if (localBundleHash.TryGetValue(bh.Key, out hash))
                    {
                        if (!hash.Equals(bh.Value))
                        {
                            _needDownLoadList.Add(bh.Key);
                        }
                    }
                    else
                        _needDownLoadList.Add(bh.Key);
                }
                for (int i = 0; i < _needDownLoadList.Count; i++)
                {
                    Debug.LogError(_needDownLoadList[i]);
                    Main.SP.StartCoroutine(DownLoadAssetBundle(_needDownLoadList[i], (s, bytes) =>
                    {
                        _needDownLoadList.Remove(s);
                       SaveToPersistentPath(s, bytes);
                    }));
                }
                SaveToPersistentPath("assetBundleHash.txt", www.downloadHandler.data);
                www.Dispose();
            }
        }

        private void SaveToPersistentPath(string path, byte[] bytes)
        {
            var psersistentPath = Path.Combine(AssetResources.PersistentUrl, path);
            System.IO.FileInfo file = new System.IO.FileInfo(psersistentPath);
            file.Directory.Create();
            System.IO.File.WriteAllBytes(file.FullName, bytes);
        }
        private IEnumerator DownLoadAssetBundle(string path, Action<string, byte[]> onloadFinish)
        {
            UnityWebRequest www = UnityWebRequest.Get("http://47.94.204.158/AB/" + path);
            yield return www.Send();
            if (www.isError)
            {
                Debug.Log(www.error);
            }
            else
            {
                var data = www.downloadHandler.data;
                www.Dispose();
                onloadFinish.Invoke(path, data);
            }
        }
    }
}
