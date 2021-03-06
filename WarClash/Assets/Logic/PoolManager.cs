﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;


public interface IPool
{
    void Reset();
}
public class Pool : Singleton<Pool>
{
    private readonly Dictionary<Type, Queue<IPool>> _poolDic = new Dictionary<Type, Queue<IPool>>();
    public void Recycle(IPool iPool)
    {
        Type type = iPool.GetType();
        if (!_poolDic.ContainsKey(type))
        {
            _poolDic[type] = new Queue<IPool>();
        }
        iPool.Reset();
        _poolDic[type].Enqueue(iPool);
    }

    public T Get<T>() where T : class, IPool 
    {
        T obj = null;
        if (_poolDic.ContainsKey(typeof(T)) && _poolDic[typeof(T)].Count > 0)
        {
            obj = _poolDic[typeof(T)].Dequeue() as T;
        }
        else
        {
            obj = Activator.CreateInstance<T>();
        }
        return obj;
    }
    public IPool Get(Type type)
    {
        IPool obj = null;
        if (_poolDic.ContainsKey(type) && _poolDic[type].Count > 0)
        {
            obj = _poolDic[type].Dequeue();
        }
        else
        {
            obj = Activator.CreateInstance(type) as IPool;
        }
        return obj;
    }

    public void Clear()
    {
        _poolDic.Clear();

    }
}
