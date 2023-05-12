using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ListRandom<T_Key>
{
    private List<T_Key> m_List;

    //----------------------------------------------------------------------------------------
    public ListRandom()
    {
        m_List = new List<T_Key>();
    }

    //----------------------------------------------------------------------------------------
    public void Add(T_Key lKey, int lAmount)
    {
        for(int i = 0; i < lAmount; i++)
        {
            m_List.Add(lKey);
        }       
    }

    //----------------------------------------------------------------------------------------
    public void Clear()
    {
        m_List.Clear();
    }

    //----------------------------------------------------------------------------------------
    public T_Key GetRandom()
    {
        int lIndex = Random.Range(0, m_List.Count);
        return m_List[lIndex];
    } 
}

