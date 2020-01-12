using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : new()
{
    private readonly Stack<T> m_stack = new Stack<T>();
    private readonly Action<T> m_actionOnGet;
    private readonly Action<T> m_actionOnRelease;

    public ObjectPool(Action<T> actionOnGet, Action<T> actionOnRelease)
    {
        m_actionOnGet = actionOnGet;
        m_actionOnRelease = actionOnRelease;
    }

    public T Get()
    {
        T item;
        if(m_stack.Count > 0)
            item = m_stack.Pop();
        else
            item = new T();

        m_actionOnGet?.Invoke(item);

        return item;
    }

    public void Release(T item)
    {
        if (item == null)
            return;

        if (m_stack.Count > 0 && ReferenceEquals(m_stack.Peek(), item))
            Debug.LogError($"Pool重复Release({typeof(T)})多次");

        m_actionOnRelease?.Invoke(item);
        m_stack.Push(item);
    }
}