using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : new()
{
    private readonly Stack<T> mStack = new Stack<T>();
    private readonly Action<T> mActionOnGet;
    private readonly Action<T> mActionOnRelease;

    /// <summary>
    /// 所有已分配的个数
    /// </summary>
    public int CountAll { get; private set; }

    /// <summary>
    /// 正在使用的个数
    /// </summary>
    public int CountUsing { get { return CountAll - CountUsable; } }

    /// <summary>
    /// 剩余可用的个数
    /// </summary>
    public int CountUsable { get { return mStack.Count; } }

    public ObjectPool(Action<T> actionOnGet, Action<T> actionOnRelease)
    {
        mActionOnGet = actionOnGet;
        mActionOnRelease = actionOnRelease;
    }

    public T Get()
    {
        T item;
        if(mStack.Count > 0)
        {
            item = mStack.Pop();
        }
        else
        {
            item = new T();
            CountAll++;
        }

        mActionOnGet?.Invoke(item);

        return item;
    }

    public void Release(T item)
    {
        if (item == null)
            return;

        if (mStack.Count > 0 && ReferenceEquals(mStack.Peek(), item))
            Debug.LogError("重复Release多次");

        mActionOnRelease?.Invoke(item);
        mStack.Push(item);
    }
}