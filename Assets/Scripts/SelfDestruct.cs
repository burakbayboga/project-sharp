using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    public float Delay;

    void Start()
    {
        Destroy(gameObject, Delay);
    }
}
