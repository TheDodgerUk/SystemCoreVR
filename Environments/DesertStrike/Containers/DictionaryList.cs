using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;


public class DictionaryList<T_Key, T_Value>
{
    private Dictionary<T_Key, List<T_Value>> m_list;
    private Dictionary<T_Key, int> m_currentIndex;

    private int m_keyIndexCount_LOOP = 0;
    private int m_keyIndex_LOOP = 0;
    private int m_ValueIndex_LOOP = 0;

    private int m_keyIndexCount_LOOP_INTERNAL = 0;
    private int m_keyIndex_LOOP_INTERNAL = 0;
    private int m_ValueIndex_LOOP_INTERNAL = 0;


    private bool HasBeenInitilisedBefore(T_Key lKey)
    {
        if (!m_list.ContainsKey(lKey))
        {
            m_list.Add(lKey, new List<T_Value>());
            m_currentIndex.Add(lKey, 0);
            return false;
        }
        return true;
    }

    /// <summary>
    /// Resets all the index to zero .
    /// </summary>
    public void ResetCurrentIndexAll()
    {
        List<T_Key> keyList = new List<T_Key>(this.m_currentIndex.Keys);
        for (int i = 0; i < keyList.Count; i++)
        {
            T_Key lKey = keyList[i];
            m_currentIndex[lKey] = 0;
        }
    }

    /// <summary>
    /// Resets Key to zero
    /// </summary>
    /// <param name="lKey">L key.</param>
    public void ResetCurrentIndexAll(T_Key lKey)
    {
        m_currentIndex[lKey] = 0;
    }

    public void ResetCurrentIndexAt(int lKeyIndex)
    {
        List<T_Key> keyList = new List<T_Key>(this.m_currentIndex.Keys);
        T_Key lKey = keyList[lKeyIndex];
        m_currentIndex[lKey] = 0;
    }


    public void SetCurrentIndex(T_Key lKey, int lIndexValue)
    {
        m_currentIndex[lKey] = lIndexValue;
    }

    /// <summary>
    /// clears all information 
    /// </summary>
    public void Clear()
    {
        m_list.Clear();
        m_currentIndex.Clear();
    }

    public DictionaryList()
    {
        m_list = new Dictionary<T_Key, List<T_Value>>();
        m_currentIndex = new Dictionary<T_Key, int>();
    }


    public DictionaryList(DictionaryList<T_Key, T_Value> lOriginal)
    {
        m_list = new Dictionary<T_Key, List<T_Value>>();
        m_currentIndex = new Dictionary<T_Key, int>();

        var lEnumerator = lOriginal.GetEnumerator();
        while (lEnumerator.MoveNext())
        {
            T_Key lKey = lEnumerator.Current.Key;
            T_Value lValue = lEnumerator.Current.Value;
            Add(lKey, lValue);
        }
    }



    public bool ContainsKey(T_Key lKey)
    {
        return m_list.ContainsKey(lKey);
    }

    public T_Key GetKeyAt(int lElementIndex)
    {
        List<T_Key> keyList = new List<T_Key>(this.m_list.Keys);
        return keyList[lElementIndex];
    }

    public int GetCurrentIndex(T_Key lKey)
    {
        HasBeenInitilisedBefore(lKey);
        m_currentIndex[lKey] = Math.Min(m_currentIndex[lKey], m_list[lKey].Count - 1);  // safety check 
        return m_currentIndex[lKey];
    }

    public T_Value GetValueAt(T_Key lKey, int lNumber)
    {
        HasBeenInitilisedBefore(lKey);
        return m_list[lKey][lNumber];
    }

    public T_Value GetValueAt(int lElemetNumber, int lElementSubNumber)
    {
        List<T_Key> keyList = new List<T_Key>(this.m_list.Keys);
        return m_list[keyList[lElemetNumber]][lElementSubNumber];

    }

    public T_Value GetCurrentValue(T_Key lKey)
    {
        m_currentIndex[lKey] = Math.Min(m_currentIndex[lKey], m_list[lKey].Count - 1);   //safety check incase its too big 	
        m_currentIndex[lKey] = Math.Max(m_currentIndex[lKey], 0);    //safety check incase its too big 	

        if (ValueCount(lKey) > 0)
        {
            return m_list[lKey][m_currentIndex[lKey]];
        }

        return default(T_Value);
    }

    public void SetValueAt(T_Key lKey, int lNumber, T_Value lValue)
    {
        m_list[lKey][lNumber] = lValue;
    }


    public T_Value this[T_Key lKey]
    {
        get
        {
            HasBeenInitilisedBefore(lKey);
            return GetCurrentValue(lKey);
        }
        set
        {
            HasBeenInitilisedBefore(lKey);

            if (ValueCount(lKey) == 0)
            {
                m_list[lKey].Add(value);
            }
            else
            {
                m_list[lKey][m_currentIndex[lKey]] = value;
            }


        }
    }

    /// <summary>
    /// Add the specified lKey and lItem.
    /// Sets the current to the top item 
    /// </summary>
    /// <param name="lKey">L key.</param>
    /// <param name="lItem">L item.</param>
    public void Add(T_Key lKey, T_Value lItem)
    {
        HasBeenInitilisedBefore(lKey);
        m_list[lKey].Add(lItem);
        m_currentIndex[lKey] = m_list[lKey].Count - 1;
    }




    public void RemoveValueViaKeyAt(T_Key lKey, int lIndex)
    {
        m_list[lKey].RemoveAt(lIndex);
        DecrementIndex(lKey);
        if (m_list[lKey].Count == 0)
        {
            m_list.Remove(lKey);
            m_currentIndex.Remove(lKey);
        }
    }

    public void RemoveValueViaAt(int lKeyIndex, int lIndex)
    {
        T_Key lKey = GetKeyAt(lKeyIndex);
        m_list[lKey].RemoveAt(lIndex);
        DecrementIndex(lKey);
        if (m_list[lKey].Count == 0)
        {
            m_list.Remove(lKey);
            m_currentIndex.Remove(lKey);
        }
    }
    public void SetIndex(T_Key lKey, int lIndex)
    {
        HasBeenInitilisedBefore(lKey);
        m_currentIndex[lKey] = lIndex;
        int lSubElementCount = ValueCount(lKey);
        if (m_currentIndex[lKey] >= lSubElementCount)
        {
            m_currentIndex[lKey] = lSubElementCount - 1;
        }
        if (m_currentIndex[lKey] < 0)
            m_currentIndex[lKey] = 0;

    }
    public void IncrementIndex(T_Key lKey)
    {
        HasBeenInitilisedBefore(lKey);
        m_currentIndex[lKey]++;
        int lSubElementCount = ValueCount(lKey);
        if (m_currentIndex[lKey] >= lSubElementCount)
        {
            m_currentIndex[lKey] = lSubElementCount - 1;
        }
    }

    public void DecrementIndex(T_Key lKey)
    {
        HasBeenInitilisedBefore(lKey);
        m_currentIndex[lKey]--;
        if (m_currentIndex[lKey] < 0)
            m_currentIndex[lKey] = 0;
    }

    public int ValueCount()
    {
        return m_list.Count;
    }

    public int ValueCount(T_Key lKey)
    {
        HasBeenInitilisedBefore(lKey);
        return m_list[lKey].Count;
    }

    public int ValueCountAt(int lElementNumber)
    {
        List<T_Key> keyList = new List<T_Key>(this.m_list.Keys);
        return m_list[keyList[lElementNumber]].Count;
    }

    public List<T_Key> GetAllKeys()
    {
        return new List<T_Key>(this.m_list.Keys);
    }


    public List<T_Value> GetAllValues()
    {
        List<T_Value> lTempList = new List<T_Value>();
        List<T_Key> keyList = new List<T_Key>(this.m_list.Keys);
        for (int lMainIndex = 0; lMainIndex < ValueCount(); lMainIndex++)
        {
            lTempList.AddRange(m_list[keyList[lMainIndex]]);
        }
        return lTempList;
    }


    public T_Value GetValuesByKeyAndIndex(T_Key lKey, int lIndex)
    {
        return m_list[lKey][lIndex];
    }

    public T_Value GetValuesByKeyIndexAndIndex(int lKeyIndex, int lIndex)
    {
        List<T_Key> keyList = new List<T_Key>(this.m_list.Keys);
        return m_list[keyList[lKeyIndex]][lIndex];
    }


    public List<T_Value> GetAllValues(T_Key lKey)
    {

        List<T_Key> keyList = new List<T_Key>(this.m_list.Keys);
        if (keyList.Contains(lKey))
            return m_list[lKey];

        List<T_Value> lReturnList = new List<T_Value>();
        return lReturnList;
    }


    public List<T_Value> GetAllValuesAt(int lIndex)
    {
        List<T_Key> keyList = new List<T_Key>(this.m_list.Keys);
        return m_list[keyList[lIndex]];
    }

    public List<T_Value> GetAllValuesAt(T_Key lName)
    {
        return m_list[lName];
    }

    public void RemoveKey(T_Key lKey)
    {
        RemoveValueViaKeyAt(lKey, GetCurrentIndex(lKey));
        DecrementIndex(lKey);
        if (m_list[lKey].Count == 0)
        {
            m_list.Remove(lKey);
            m_currentIndex.Remove(lKey);
        }
    }

    public void RemoveValue(T_Value lValue)
    {

        var lInternal = GetEnumerator_INTERNAL();
        while (lInternal.MoveNext_INTERNAL())
        {
            if (lInternal.Current_INTERNAL.Value.ToString() == lValue.ToString())
            {
                RemoveValueViaAt(m_keyIndex_LOOP_INTERNAL, m_ValueIndex_LOOP_INTERNAL);
                break; // only delete one object  // if carry on , gets errors as it goes over a deleteed item 
            }
        }
    }



    public bool TryGetValuesByKeyAndIndex(T_Key lKey, int lValueIndex, out T_Value lValue)
    {
        lValue = default(T_Value);
        if (ContainsKey(lKey) == false)
            return false;

        int lValueCount = ValueCount(lKey);
        if (lValueIndex >= lValueCount)
            return false;


        lValue = GetValuesByKeyAndIndex(lKey, lValueIndex);
        return true;
    }

    public void SwapValues(T_Key lKey, int lFirstValueIndex, int lSecondValueIndex)
    {
        T_Value lFirstValue = GetValueAt(lKey, lFirstValueIndex);
        T_Value lSecondValue = GetValueAt(lKey, lSecondValueIndex);

        SetValueAt(lKey, lFirstValueIndex, lSecondValue);
        SetValueAt(lKey, lSecondValueIndex, lFirstValue);

    }

    public bool TryGetValueByKeyAndValueIndex(T_Key lKey, int lValueIndex, out T_Value lValue)
    {
        lValue = default(T_Value);
        if (m_list.ContainsKey(lKey) == false)
            return false;

        if (ValueCount(lKey) < lValueIndex)
            return false;


        lValue = GetValuesByKeyAndIndex(lKey, lValueIndex);
        return true;

    }




    public bool TryGetValue(T_Key lKey, out T_Value value)
    {
        value = default(T_Value);
        List<T_Value> lTemp;
        if (m_list.TryGetValue(lKey, out lTemp))
        {
            value = GetCurrentValue(lKey);
            return true;
        }
        return false;
    }



    // loop through all setup
    public int Count
    {
        get
        {
            int lCount = 0;
            for (int i = 0; i < ValueCount(); i++)
            {
                lCount += ValueCountAt(i);
            }
            return lCount;
        }
    }








    //*********************************************************************************************************
    //							GetEnumerator stuff
    //*********************************************************************************************************
    private int GetEnumerator_Count()
    {
        int lCount = 0;
        for (int i = 0; i < ValueCount(); i++)
        {
            lCount += ValueCountAt(i);
        }
        return lCount;
    }

    private void GetEnumerator_Increment()
    {
        m_ValueIndex_LOOP++;
        if (m_ValueIndex_LOOP >= ValueCountAt(m_keyIndex_LOOP))
        {
            m_keyIndex_LOOP++;
            m_ValueIndex_LOOP = 0;
        }
    }

    private void GetEnumerator_Increment_INTERNAL()
    {
        m_ValueIndex_LOOP_INTERNAL++;
        if (m_ValueIndex_LOOP_INTERNAL >= ValueCountAt(m_keyIndex_LOOP_INTERNAL))
        {
            m_keyIndex_LOOP_INTERNAL++;
            m_ValueIndex_LOOP_INTERNAL = 0;
        }
    }


    private void GetEnumerator_Reset(bool isInternal)
    {
        List<T_Key> keyList = new List<T_Key>(this.m_list.Keys);
        if (isInternal == false)
        {
            m_keyIndexCount_LOOP = keyList.Count;
            m_keyIndex_LOOP = 0; // 
            m_ValueIndex_LOOP = -1; //minus one  because its set then first thing is MoveNext() which increments it 
        }
        else
        {
            m_keyIndexCount_LOOP_INTERNAL = keyList.Count;
            m_keyIndex_LOOP_INTERNAL = 0; // 
            m_ValueIndex_LOOP_INTERNAL = -1; //minus one  because its set then first thing is MoveNext() which increments it 

        }
    }


    private T_Key GetEnumerator_CurrentKey()
    {
        return GetKeyAt(m_keyIndex_LOOP);
    }

    private T_Value GetEnumerator_CurrentValue()
    {
        return GetValueAt(m_keyIndex_LOOP, m_ValueIndex_LOOP);
    }

    private T_Key GetEnumerator_CurrentKey_INTERNAL()
    {
        return GetKeyAt(m_keyIndex_LOOP_INTERNAL);
    }

    private T_Value GetEnumerator_CurrentValue_INTERNAL()
    {
        return GetValueAt(m_keyIndex_LOOP_INTERNAL, m_ValueIndex_LOOP_INTERNAL);
    }


    public DictionaryList<T_Key, T_Value> GetEnumerator()
    {
        GetEnumerator_Reset(false);
        return this;
    }

    private DictionaryList<T_Key, T_Value> GetEnumerator_INTERNAL()
    {
        GetEnumerator_Reset(true);
        return this;
    }


    public bool MoveNext()
    {
        GetEnumerator_Increment();
        return (m_keyIndex_LOOP < m_keyIndexCount_LOOP);
    }

    public int GetEnumeratorCurrentKeyIndex()
    {
        return m_keyIndex_LOOP;
    }

    public int GetEnumeratorCurrentValueIndex()
    {
        return m_ValueIndex_LOOP;
    }


    private bool MoveNext_INTERNAL()
    {
        GetEnumerator_Increment_INTERNAL();
        return (m_keyIndex_LOOP_INTERNAL < m_keyIndexCount_LOOP_INTERNAL);
    }

    public KeyValuePair<T_Key, T_Value> Current
    {
        get
        {
            return new KeyValuePair<T_Key, T_Value>(GetEnumerator_CurrentKey(), GetEnumerator_CurrentValue());
        }
    }

    private KeyValuePair<T_Key, T_Value> Current_INTERNAL
    {
        get
        {
            return new KeyValuePair<T_Key, T_Value>(GetEnumerator_CurrentKey_INTERNAL(), GetEnumerator_CurrentValue_INTERNAL());
        }
    }

}

