using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public bool format3x3;
    public int formatNumber;
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        Destroy(GameObject.FindGameObjectWithTag("GameManager"), 2.5f);
    }
}