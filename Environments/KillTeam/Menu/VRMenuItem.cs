using System;
using UnityEngine;

[Serializable]
public class VRMenuItem
{
    public Sprite thumbnail;
    public GameObject prefab;

    public VRMenuItem(Sprite thumbnail, GameObject prefab)
    {
        this.thumbnail = thumbnail;
        this.prefab = prefab;
    }
}