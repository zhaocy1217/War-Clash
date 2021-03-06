﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
[SelectionBase]
public class MapEditCopy : MonoBehaviour
{
    private MeshFilter mf;
    public int width = 9;
    public int height = 9;
    public float cell_width = 2f;
    public float cell_height = 1f;
    private int[,] data;
    private MeshRenderer mr;
    private Material mat;
    private Mesh m;
    void Start()
    {

        //Generate();
    }
    public void Generate()
    {
        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();
        mat = mr.sharedMaterial;
        data = new int[width - 1, height - 1];
        m = new Mesh();
        Vector3[] vs = new Vector3[width * height];
        Vector2[] uvs = new Vector2[width * height];
        List<int> ts = new List<int>();

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                vs[i * width + j] = new Vector3(j * cell_width, 0, i * cell_height);
                uvs[i * width + j] = new Vector2(j / (float)(width - 1), i / (float)(height - 1));
                if (j < width - 1)
                {
                    if (i == 0)
                    {
                        ts.Add(i * width + j);
                        ts.Add((i + 1) * width + j + 1);
                        ts.Add((i) * width + j + 1);
                    }
                    else if (i == height - 1)
                    {
                        ts.Add(i * width + j);
                        ts.Add((i) * width + j + 1);
                        ts.Add((i - 1) * width + j);
                    }
                    else
                    {
                        ts.Add(i * width + j);
                        ts.Add((i) * width + j + 1);
                        ts.Add((i - 1) * width + j);

                        ts.Add(i * width + j);
                        ts.Add((i + 1) * width + j + 1);
                        ts.Add((i) * width + j + 1);
                    }
                }
            }
        }
        m.vertices = vs;
        m.triangles = ts.ToArray();
        m.uv = uvs;
        m.RecalculateNormals();
        mf.mesh = m;
        var mc = gameObject.GetComponent<MeshCollider>();
        if(mc)
        {
            GameObject.DestroyImmediate(mc);
        }
        var bc = gameObject.GetComponent<BoxCollider>();
        if (bc != null)
        {
            bc.center = mr.bounds.center - transform.position;
            bc.size = mr.bounds.size;
        }
        if (bc == null)
        {
            gameObject.AddComponent<BoxCollider>();
        }
        UpdateImage();
    }
    public void OnHit(Vector3 p)
    {
        if (mat != null)
        {
            Vector3 relative_p = p - gameObject.transform.position;
            int x = Mathf.FloorToInt(relative_p.x / cell_width);
            int z = Mathf.FloorToInt(relative_p.z / cell_height);
            data[x, z] = 1;
            UpdateImage();
        }
    }
    // Update is called once per frame
    public void Raycast(bool fromEditor)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);// HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        if(fromEditor)
        {
           // ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        }
        var hits = Physics.RaycastAll(ray, 100);
        for (int i = 0; i < hits.Length; i++)
        {
            var hit = hits[i];
            var tran = gameObject.transform;
            if (tran == hit.transform)
            {
                OnHit(hit.point);
            }
        }
    }
    void OnMouseDrag()
    {
        Raycast(false);
    }
    void OnMouseMove()
    {
        Raycast(false);
    }
    void UpdateImage()
    {
        Texture2D tex = new Texture2D(width-1, height-1, TextureFormat.ARGB32, false);
        for (int i = 0; i < height-1; i++)
        {
            for (int j = 0; j < width-1; j++)
            {
                if(data[i, j] != 0)
                {
                    tex.SetPixel(i, j, Color.red);
                }
                else
                {
                    tex.SetPixel(i, j, Color.gray);
                }
            }
        }
        tex.filterMode = FilterMode.Point;
        tex.Apply();
        mat.SetTexture("_MainTex", tex);
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
       
        for (int i = 0; i < height-1; i++)
        {
            for (int j = 0; j < width - 1; j++)
            {
                int a = data[j, i];
                if(a == 0)
                {
                    Gizmos.color = Color.red;
                }
                else
                {
                    Gizmos.color = Color.green;
                }
                Gizmos.DrawCube(new Vector3(j * cell_width, 0, i * cell_height)+ new Vector3(cell_width, 0.001f, cell_height)/2, 
                    new Vector3(cell_width, 0.001f, cell_height));
            }
        }
       

        Gizmos.color = Color.cyan;
        for (int i = 0; i < height; i++)
        {
            Gizmos.DrawLine(transform.position + new Vector3(0,0,i*cell_height), transform.position + new Vector3(cell_width * (width-1), 0, i * cell_height));
        }
        for (int i = 0; i < width; i++)
        {
            Gizmos.DrawLine(transform.position + new Vector3(i*cell_width, 0, 0), transform.position + new Vector3(i * cell_width, 0, (height-1) * cell_height));
        }
    }
}
