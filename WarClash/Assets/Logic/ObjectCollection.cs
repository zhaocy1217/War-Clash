using System;
using Logic;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Collections;
using FastCollections;
using Logic.LogicObject;
using UnityEngine;
using Logic.Components;

namespace Logic.LogicObject
{
    public class ObjectCollection<TKey, TValue> where TValue : class
    {
        private Dictionary<TKey, TValue> _coll = new BiDictionary<TKey, TValue>();
        private LinkedList<TValue> _valList = new LinkedList<TValue>();
        private LinkedList<IUpdate> _updateItems = new LinkedList<IUpdate>();
        private LinkedList<IFixedUpdate> _fixedUpdateItems = new LinkedList<IFixedUpdate>();

        private Dictionary<Type, HashSet<TValue>> compDic = new Dictionary<Type, HashSet<TValue>>();

        public void AddComponent(Type t, TValue owner)
        {
            HashSet<TValue> set;
            if(compDic.TryGetValue(t, out set))
            {
                set.Add(owner);
            }
            else
            {
                set = new HashSet<TValue>();
                compDic.Add(t, set);
                set.Add(owner);
            }
        }
        public void RemoveComponent(Type t, TValue owner)
        {
            HashSet<TValue> set;
            if (compDic.TryGetValue(t, out set))
            {
                set.Remove(owner);
            }
        }
        public HashSet<TValue> GetObjectsWithComponent<T>() where T: BaseComponent
        {
            var t = typeof(T);
            HashSet<TValue> set;
            if(compDic.TryGetValue(t, out set))
            {
            }
            return set;
        }
        public TValue GetObjectWithComponent<T>() where T : BaseComponent
        {
            var t = typeof(T);
            HashSet<TValue> set;
            if (compDic.TryGetValue(t, out set))
            {
                return set.First();
            }
            return null;
        }
        public void AddObject(TKey key, TValue value)
        {
            if (!_coll.ContainsKey(key))
            {
                _coll.Add(key, value);
                _valList.AddLast(value);
                if(value is IUpdate)
                {
                    _updateItems.AddLast((IUpdate)value);
                }
                if (value is IFixedUpdate)
                {
                    _fixedUpdateItems.AddLast((IFixedUpdate)value);
                }
            }
            else
            {
                DLog.LogError(key.ToString() + " 已经存在");
            }
        }
        public T GetObject<T>() where T : TValue
        {
            if (_valList.Count == 0) return default(T);
            else
            {
                var node = _valList.First;
                while (node != null)
                {
                    if (node.Value is T)
                        return (T)node.Value;
                    node = node.Next;
                }
            }
            return default(T);
        }
        public T GetObject<T>(TKey key) where T : TValue
        {
            TValue val = null;
            if (_coll.TryGetValue(key, out val) && val is T)
            {
                return (T)val;
            }
            return default(T);
        }
        public TValue GetObject(TKey key)
        {
            TValue val = null;
            if (_coll.TryGetValue(key, out val))
            {
                return val;
            }
            return null;
        }

        public void RemoveObject(TKey key)
        {
            TValue v = null;
            if (_coll.TryGetValue(key, out v))
            {
                foreach (var item in compDic)
                {
                    item.Value.Remove(v);
                }
                _valList.Remove(v);
                _coll.Remove(key);
                if(v is IUpdate)
                {
                    _updateItems.Remove((IUpdate)v);
                }
                else if (v is IFixedUpdate)
                {
                    _fixedUpdateItems.Remove((IFixedUpdate)v);
                }
            }
            else
            {
                DLog.LogError(key.ToString()+" 不存在");
            }
        }

        public void ForEachDo<T>(Action<T> action) where T : TValue
        {
            if (_valList.Count == 0) return;
            else
            {
                var node = _valList.First;
                while (node!=null)
                {
                    if (node.Value is T)
                        action.Invoke((T)(node.Value));
                    node = node.Next;
                }
            }
        }
        public T ForEachDo<T>(Func<T, bool> action) where T : TValue
        {
            if (_valList.Count == 0) return default(T);
            else
            {
                var node = _valList.First;
                while (node != null)
                {
                    if (node.Value is T && action.Invoke((T)node.Value))
                        return (T)node.Value;
                    node = node.Next;
                }
            }
            return default(T);
        }

        public void Update(float deltaTime)
        {
            OnUpdate(deltaTime);
            if (_updateItems.Count == 0) return;
            else
            {
                var node = _updateItems.First;
                while (node != null)
                {
                    var u = node.Value;
                    u.Update(deltaTime);
                    node = node.Next;
                }
            }
        }

        public void FixedUpdate(long deltaTime)
        {
            OnFixedUpdate(deltaTime);
            if (_fixedUpdateItems.Count == 0) return;
            else
            {
                var node = _fixedUpdateItems.First;
                while (node != null)
                {
                    var fu = node.Value;
                    (fu).FixedUpdate(deltaTime);
                    node = node.Next;
                }
            }
        }
        public virtual void OnUpdate(float deltaTime)
        {

        }

        public virtual void OnFixedUpdate(long deltaTime)
        {

        }
    }

}


namespace Logic.Objects
{
    //// 对象聚合类, 保证遍历时插入和删除操作时安全的
    //public class ObjectCollection<TKey, TValue> where TValue : class
    //{
    //    private class Pair<TPKey, TPValue>
    //    {
    //        public TPKey key { get; private set; }
    //        public TPValue val { get; private set; }

    //        public Pair(TPKey k, TPValue v)
    //        {
    //            this.key = k;
    //            this.val = v;
    //        }
    //    }

    //    public class ValuePack<TPValue>
    //    {
    //        public TPValue Val;
    //        public bool Enable;

    //        public ValuePack(TPValue val)
    //        {
    //            this.Val = val;
    //            this.Enable = true;
    //        }
    //    }

    //    private static LinkedList<Type> cacheTValueType = new LinkedList<Type>();
    //    static ObjectCollection()
    //    {
    //        Type valType = typeof(TValue);
    //        Assembly ass = Assembly.GetAssembly(valType);
    //        foreach (Type type in ass.GetTypes())
    //        {
    //            if (type.IsSubclassOf(valType) || type == valType)
    //            {
    //                cacheTValueType.AddLast(type);
    //            }
    //        }
    //    }

    //    private Dictionary<Type, Dictionary<TKey, ValuePack<TValue>>> objectColl;
    //    private List<Pair<TKey, TValue>> delayAdd;
    //    private List<TKey> delayDel;
    //    private int traverseLevel = 0;

    //    public Dictionary<Type, Dictionary<TKey, TValue>> GetAllValues()
    //    {
    //        Dictionary<Type, Dictionary<TKey, TValue>> values = new Dictionary<Type, Dictionary<TKey, TValue>>();

    //        foreach (KeyValuePair<Type, Dictionary<TKey, ValuePack<TValue>>> pair in objectColl)
    //        {
    //            Dictionary<TKey, TValue> innerDic = new Dictionary<TKey, TValue>();

    //            foreach (KeyValuePair<TKey, ValuePack<TValue>> innerPair in pair.Value)
    //            {
    //                innerDic.Add(innerPair.Key, innerPair.Value.Val);
    //            }

    //            values.Add(pair.Key, innerDic);
    //        }
    //        return values;
    //    }

    //    public ObjectCollection()
    //    {
    //        objectColl = new Dictionary<Type, Dictionary<TKey, ValuePack<TValue>>>();
    //        delayAdd = new List<Pair<TKey, TValue>>();
    //        delayDel = new List<TKey>();
    //        delayAdd.Clear();
    //        delayDel.Clear();

    //        foreach (Type type in cacheTValueType)
    //        {
    //            objectColl.Add(type, new Dictionary<TKey, ValuePack<TValue>>());
    //        }
    //    }

    //    private bool IsTraversing { get { return traverseLevel > 0; } }

    //    private bool AddObjectImmediately(TKey key, TValue val)
    //    {
    //        bool ret = false;
    //        foreach (KeyValuePair<Type, Dictionary<TKey, ValuePack<TValue>>> kvp in objectColl)
    //        {
    //            if (kvp.Key.IsInstanceOfType(val))
    //            {
    //                if (!kvp.Value.ContainsKey(key))
    //                {
    //                    kvp.Value.Add(key, new ValuePack<TValue>(val));

    //                    ret = true;
    //                }
    //            }
    //        }
    //        return ret;
    //    }

    //    private bool RemoveObjectImmediately(TKey key)
    //    {
    //        bool ret = false;
    //        foreach (Dictionary<TKey, ValuePack<TValue>> dic in objectColl.Values)
    //        {
    //            if (dic.ContainsKey(key))
    //            {
    //                dic.Remove(key);
    //                ret = true;
    //            }
    //        }
    //        return ret;
    //    }

    //    protected void AddObject(TKey key, TValue val)
    //    {
    //        if (IsTraversing)
    //        {
    //            if (!HasObject(key))
    //            {
    //                delayAdd.Add(new Pair<TKey, TValue>(key, val));
    //            }
    //            else
    //            {
                    
    //            }
    //        }
    //        else
    //        {
    //            AddObjectImmediately(key, val);
    //        }
    //    }

    //    protected void RemoveObject(TKey key)
    //    {
    //        if (IsTraversing)
    //        {
    //            bool b1 = false;
    //            for (int i = 0; i < delayAdd.Count; i++)
    //            {
    //                if (null != delayAdd[i] && key.Equals(delayAdd[i].key))
    //                {
    //                    delayAdd[i] = null;
    //                    b1 = true;
    //                    break;
    //                }
    //            }
    //            if (!b1)
    //            {
    //                if (!delayDel.Contains(key))
    //                {
    //                    bool b2 = false;
    //                    foreach (Dictionary<TKey, ValuePack<TValue>> dic in objectColl.Values)
    //                    {
    //                        if (dic.ContainsKey(key))
    //                        {
    //                            dic[key].Enable = false;
    //                            b2 = true;
    //                        }
    //                    }
    //                    if (b2) delayDel.Add(key);
    //                }
    //            }
    //        }
    //        else
    //        {
    //            RemoveObjectImmediately(key);
    //        }
    //    }

    //    public int Count<TDeriveTValue>() where TDeriveTValue : class, TValue
    //    {
    //        Type type = typeof(TDeriveTValue);
    //        if (!objectColl.ContainsKey(type))
    //            return 0;
    //        return objectColl[type].Count;
    //    }
    //    public void Update(float deltaTime)
    //    {
    //        OnUpdate(deltaTime);
    //        traverseLevel++;
    //        var dic = objectColl[typeof(TValue)];
    //        if(dic != null)
    //        {
    //            foreach (var item in dic)
    //            {
    //                if (item.Value != null && item.Value.Val is IUpdate)
    //                {
    //                    IUpdate so = (IUpdate)item.Value.Val;
    //                    so.Update(deltaTime);
    //                }
    //            }
    //        }
    //        DecTraverseLevel();
    //    }

    //    public void FixedUpdate(long deltaTime)
    //    {
    //        OnFixedUpdate(deltaTime);
    //        traverseLevel++;
    //        var dic = objectColl[typeof(TValue)];
    //        if (dic != null)
    //        {
    //            foreach (var item in dic)
    //            {
    //                if (item.Value!=null && item.Value.Val is IFixedUpdate)
    //                {
    //                    IFixedUpdate so = (IFixedUpdate)item.Value.Val;
    //                    so.FixedUpdate(deltaTime);
    //                }
    //            }
    //        }
    //        DecTraverseLevel();
    //    }
    //    public virtual void OnUpdate(float deltaTime)
    //    {

    //    }

    //    public virtual void OnFixedUpdate(long deltaTime)
    //    {
            
    //    }
    //    public TDeriveTValue GetObject<TDeriveTValue>() where TDeriveTValue : class, TValue
    //    {
    //        Type type = typeof(TDeriveTValue);
    //        Dictionary<TKey, ValuePack<TValue>> val;
    //        if (objectColl.TryGetValue(type, out val))
    //        {
    //            if (val.Count > 0)
    //            {
    //                ValuePack<TValue> valPack = val.First().Value;
    //                if (valPack.Enable)
    //                {
    //                    return valPack.Val as TDeriveTValue;
    //                }
    //                else
    //                {
    //                    return null;
    //                }
    //            }
    //            else return null;
    //        }
    //        if (IsTraversing)
    //        {
    //            foreach (Pair<TKey, TValue> pair in delayAdd)
    //            {
    //                if (pair.val is TDeriveTValue)
    //                {
    //                    return pair.val as TDeriveTValue;
    //                }
    //            }
    //        }
    //        return null;
    //    }
    //    public TDeriveTValue GetObject<TDeriveTValue>(TKey key) where TDeriveTValue : class, TValue
    //    {
    //        Type type = typeof(TDeriveTValue);
    //        Dictionary<TKey, ValuePack<TValue>> val;
    //        if (objectColl.TryGetValue(type, out val))
    //        {
    //            ValuePack<TValue> valPack;
    //            if (val.TryGetValue(key, out valPack))
    //            {
    //                if (valPack.Enable)
    //                {
    //                    return valPack.Val as TDeriveTValue;
    //                }
    //            }
    //        }
    //        if (IsTraversing)
    //        {
    //            foreach (Pair<TKey, TValue> pair in delayAdd)
    //            {
    //                if (null != pair && key.Equals(pair.key))
    //                {
    //                    return pair.val as TDeriveTValue;
    //                }
    //            }
    //        }
    //        return null;
    //    }

    //    public TValue GetObject(TKey key)
    //    {
    //        return GetObject<TValue>(key);
    //    }

    //    public bool HasObject<TDeriveTValue>(TKey key) where TDeriveTValue : class, TValue
    //    {
    //        return null != GetObject<TDeriveTValue>(key);
    //    }

    //    public bool HasObject(TKey key)
    //    {
    //        return HasObject<TValue>(key);
    //    }

    //    private void DecTraverseLevel()
    //    {
    //        if (traverseLevel <= 0)
    //            return;
    //        traverseLevel--;
    //        if (0 == traverseLevel)
    //        {
    //            foreach (TKey key in delayDel)
    //            {
    //                RemoveObjectImmediately(key);
    //            }
    //            foreach (Pair<TKey, TValue> pair in delayAdd)
    //            {
    //                if (null != pair)
    //                {
    //                    AddObjectImmediately(pair.key, pair.val);
    //                }
    //            }
    //            delayDel.Clear();
    //            delayAdd.Clear();
    //        }
    //    }

    //    public delegate bool BoolAction<DeriveTValue>(DeriveTValue val) where DeriveTValue : class, TValue;
    //    public DeriveTValue ForEachDo<DeriveTValue>(BoolAction<DeriveTValue> action) where DeriveTValue : class, TValue
    //    {
    //        if (null == action) return null;
    //        traverseLevel++;
    //        Type type = typeof(DeriveTValue);
    //        DeriveTValue ret = null;
    //        Dictionary<TKey, ValuePack<TValue>> dic;
    //        if (objectColl.TryGetValue(type, out dic))
    //        {
    //            foreach (var valPack in dic)
    //            {
    //                if (!valPack.Value.Enable) continue;
    //                if (action(valPack.Value.Val as DeriveTValue))
    //                {
    //                    ret = valPack.Value.Val as DeriveTValue;
    //                    break;
    //                }
    //            }
    //        }

    //        if (null == ret)
    //        {
    //            for (int i = 0; i < delayAdd.Count; i++)
    //            {
    //                Pair<TKey, TValue> pair = delayAdd[i];
    //                if (null == pair) continue;
    //                if (!(pair.val is DeriveTValue)) continue;
    //                if (action(pair.val as DeriveTValue))
    //                {
    //                    ret = pair.val as DeriveTValue;
    //                    break;
    //                }
    //            }
    //        }
    //        DecTraverseLevel();
    //        return ret;
    //    }
    //    public TValue ForEachDo(BoolAction<TValue> action)
    //    {
    //        return ForEachDo<TValue>(action);
    //    }
    //    public delegate void VoidAction<DeriveTValue>(DeriveTValue val) where DeriveTValue : class, TValue;
    //    public void ForEachDo<DeriveTValue>(VoidAction<DeriveTValue> action) where DeriveTValue : class, TValue
    //    {
    //        if (null == action) return;
    //        traverseLevel++;
    //        Type type = typeof(DeriveTValue);
    //        if (objectColl.ContainsKey(type))
    //        {
    //            foreach (KeyValuePair<TKey, ValuePack<TValue>> valPack in objectColl[type])
    //            {
    //                if (!valPack.Value.Enable) continue;
    //                action(valPack.Value.Val as DeriveTValue);
    //            }
    //        }
    //        for (int i = 0; i < delayAdd.Count; i++)
    //        {
    //            Pair<TKey, TValue> pair = delayAdd[i];
    //            if (null == pair) continue;
    //            if (!(pair.val is DeriveTValue)) continue;
    //            action(pair.val as DeriveTValue);
    //        }
    //        DecTraverseLevel();
    //    }
    //    public void ForEachDo(VoidAction<TValue> action)
    //    {
    //        ForEachDo<TValue>(action);
    //    }

    //    // add by cx 2014-10-9
    //    public TValue[] GetValues()
    //    {
    //        List<TValue> values = new List<TValue>();
    //        foreach (var group in this.objectColl)
    //        {
    //            foreach (var _object in group.Value)
    //            {
    //                if (_object.Value.Enable)
    //                {
    //                    values.Add(_object.Value.Val);
    //                }
    //            }
    //        }

    //        return values.ToArray();
    //    }

    //}
}

