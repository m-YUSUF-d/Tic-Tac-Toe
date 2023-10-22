using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
    public int whoIsTurn, scoreX, scoreO, checkBox;
    public TextMeshProUGUI textX, textO, results;
    public bool is3x3Mode;

    public Menu menu;
    MenuManager menuManager;

    public GameObject X, O;
    public GameObject[] threeSquare;
    public GameObject[] fourSquare;

    [HideInInspector]
    public string whoIsTurnText;

    void Awake()
    {
        GameObject[] managers = GameObject.FindGameObjectsWithTag("GameManager");
        if (managers.Length == 0) Destroy(gameObject);
        else gameObject.SetActive(true);
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        menuManager = FindObjectOfType<MenuManager>();
        whoIsTurn = 0;

        is3x3Mode = menuManager.format3x3;

        textX.text = menuManager.ScoreX.ToString();
        textO.text = menuManager.ScoreO.ToString();

        if (is3x3Mode)
        {
            for (int i = 0; i < threeSquare.Length; i++)
            {
                threeSquare[i].SetActive(true);
            }

            for (int i = 0; i < fourSquare.Length; i++)
            {
                fourSquare[i].SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < threeSquare.Length; i++)
            {
                threeSquare[i].SetActive(false);
            }

            for (int i = 0; i < fourSquare.Length; i++)
            {
                fourSquare[i].SetActive(true);
            }
        }
    }
}
