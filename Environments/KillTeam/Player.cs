using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KillTeam
{
    public class Player : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            this.AddComponent<TableControl>();
        }
    }
}