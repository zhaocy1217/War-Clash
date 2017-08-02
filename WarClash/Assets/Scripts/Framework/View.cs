﻿
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class View : IEventDispatcher
{
    private List<View> subViews = new List<View>();
    public GameObject go { get; set; }
    public string name { get; set; }

    public virtual void OnInit(GameObject go) { }

    public virtual void OnShow(object para) { }

    public virtual void OnHide() { }

    public virtual void OnDispose() { }
    public virtual void OnUpdate() { }

    public T GetComponent<T>(string path) where T : Component
    {
        var t = go.transform.Find(path);
        if(t!=null)
        {
            var c = t.GetComponent<T>();
            return c;
        }
        return null;
    }
    public T MakeSubView<T>(string path) where T : View
    {
        var trans = go.transform.Find(path);
        return MakeSubView<T>(trans.gameObject);
    }
    public T MakeSubView<T>(GameObject subg) where T : View
    {
        T v = Activator.CreateInstance<T>();
        v.Init(subg);
        subViews.Add(v);
        return v;
    }
    public void ListenEvent(int id, EventMsgHandler e)
    {
        EventDispatcher.ListenEvent(id, e);
    }

    public void DelEvent(int id, EventMsgHandler e)
    {
        EventDispatcher.DelEvent(id, e);
    }

    public void FireEvent(int id, object sender, EventMsg m)
    {
        EventDispatcher.FireEvent(id, sender, m);
    }

    public void Show(object para)
    {
        OnShow(para);
    }

    public void Hide()
    {
        FireEvent((int)EventList.HideUI, this, EventGroup.NewArg<EventSingleArgs<View>, View>(this));
    }
    public void Init(GameObject go)
    {
        this.go = go;
        OnInit(go);
    }
    public void Update()
    {
        UpdateView(this);
    }

    private void UpdateView(View v)
    {
        v.OnUpdate();
        for (int i = 0; i < v.subViews.Count; i++)
        {
            UpdateView(v.subViews[i]);
        }
    }
    public void Dispose()
    {
        DisposeView(this);
        GameObject.Destroy(go);
    }
    private void DisposeView(View v)
    {
        v.OnDispose();
        for (int i = 0; i < v.subViews.Count; i++)
        {
            DisposeView(v.subViews[i]);
        }
    }
}

