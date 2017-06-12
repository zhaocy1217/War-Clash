﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public class Test_06_11 : MonoBehaviour
{

    private Mesh mesh;
    private Dictionary<Transform, List<int>> t_l = new Dictionary<Transform, List<int>>();
    private Dictionary<Transform, List<Vector3>> t_o = new Dictionary<Transform, List<Vector3>>();
    private Matrix4x4 world2localM;
    // Use this for initialization
    void Start()
    {
        world2localM = transform.worldToLocalMatrix;
        mesh = GetComponent<MeshFilter>().mesh;
        vertexs = new Vector3[mesh.vertices.Length];
        string str = File.ReadAllText(Application.dataPath + "/" + gameObject.name + ".json");
        VAData o = Newtonsoft.Json.JsonConvert.DeserializeObject<VAData>(str);
        foreach (var item in o.dic)
        {
            Transform t = GetChild(transform.parent, item.Key);
            if(t != null)
            {
                t_l[t] = item.Value;
            }
            t_o[t] = new List<Vector3>();
            for (int i = 0; i < item.Value.Count; i++)
            {
                VAData.V v =  o.offset[item.Key][i];
                t_o[t].Add(new Vector3(v.x, v.y, v.z));
            }
       
        }
    }
    public Transform GetChild(Transform parent, string name)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            var t = parent.GetChild(i);
            Transform target;
            if (t.name == name)
            {
                return t;
            }
            else
            {
                target = GetChild(t, name);
            }
            if(target!= null)
            {
                return target;
            }
        }
        return null;

    }
    Vector3[] vertexs;
    // Update is called once per frame
    void Update()
    {
        foreach (var item in t_l)
        {
            for (int i = 0; i < item.Value.Count; i++)
            {
                vertexs[item.Value[i]] = world2localM.MultiplyPoint(item.Key.localToWorldMatrix.MultiplyPoint(t_o[item.Key][i]));
            }
        }

        mesh.vertices = vertexs;
  //      mesh.RecalculateNormals();
    }
}

public class VAData
{
    public struct V
    {
        public float x, y, z;
    }
    public Dictionary<string, List<int>> dic = new Dictionary<string, List<int>>();
    public Dictionary<string, List<V>> offset = new Dictionary<string, List<V>>();
}