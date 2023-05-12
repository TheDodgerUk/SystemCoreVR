using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;


public class DictionaryCount<T_Key, T_Value>
{
    private Dictionary<T_Key, int> m_currentIndex;

    public DictionaryCount()
    {
        m_currentIndex = new Dictionary<T_Key, int>();
    }


    private void _CheckIfExistsAndInitilise(T_Key lKey)
    {
        if (!m_currentIndex.ContainsKey(lKey))
        {
            m_currentIndex.Add(lKey, 0);
        }
    }






    /// <summary>
    /// clears all information 
    /// </summary>
    public void Clear()
    {
        m_currentIndex.Clear();
    }
    //---------------------------------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    /// <param name="lKey"></param>
    public void ClearCount(T_Key lKey)
    {
        _CheckIfExistsAndInitilise(lKey);
        m_currentIndex[lKey] = 0;
    }










    public void SetCount(T_Key lKey, int lIndexValue)
    {
        _CheckIfExistsAndInitilise(lKey);
        m_currentIndex[lKey] = lIndexValue;
    }




    public int GetCount(T_Key lKey)
    {
        _CheckIfExistsAndInitilise(lKey);
        return m_currentIndex[lKey];
    }

    public int GetCountAt(int lElementIndex)
    {
        List<T_Key> keyList = new List<T_Key>(this.m_currentIndex.Keys);
        return m_currentIndex[keyList[lElementIndex]];
    }


    public void Add(T_Key lKey)
    {
        _CheckIfExistsAndInitilise(lKey);
        m_currentIndex[lKey]++;
    }

    public void Remove(T_Key lKey)
    {
        _CheckIfExistsAndInitilise(lKey);
        if (m_currentIndex[lKey] > 0)
        {
            m_currentIndex[lKey]--;
        }
    }



}

