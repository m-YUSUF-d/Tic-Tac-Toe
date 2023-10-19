using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class X_O : MonoBehaviour
{
    public void Start()
    {
        RaycastHit[] hits;
        hits = Physics.RaycastAll(transform.position, transform.up, 20);

        for (int i = 0; i < hits.Length; i++)
        {

        }
    }
}
