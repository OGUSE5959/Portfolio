using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool<T> where T : Component
{
    Queue<T> m_pool = new Queue<T>();
    public Queue<T> Pool { get { return m_pool; } }
    Func<T> m_createFunc;
    int m_presetCount;
    public int Count { get { return m_pool.Count; } }

    public GameObjectPool() { }
    public GameObjectPool(int count, Func<T> createFuc)
    {
        m_presetCount = count;
        m_createFunc = createFuc;
        Allocation();
    }
    public void CreatePool(int count, Func<T> createFuc)
    {
        m_presetCount = count;
        m_createFunc = createFuc;
        Allocation();
    }

    public void Clear() => m_pool.Clear();
    void Allocation()
    {
        for(int i = 0; i  < m_presetCount; i++)
        {
            var obj = m_createFunc();
            m_pool.Enqueue(obj);
        }       
    }
    public T New()
    {
        // var obj = m_createFunc();
        return m_createFunc();
    }
    public T Get()
    {
        if(m_pool.Count == 0)
            return New();

        return m_pool.Dequeue();
    }
    public void Set(T obj)
    {
        m_pool.Enqueue(obj);
    }
}
