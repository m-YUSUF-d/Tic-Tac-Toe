using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Ray : MonoBehaviour
{
    public RaycastHit[] hits;
    public Menu menu;
    public GameManager manager;
    public MenuManager mManager;
    public int rayDistance = 100;

    void Start()
    {
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        mManager = GameObject.FindGameObjectWithTag("MenuManager").GetComponent<MenuManager>();
    }

    void Update()
    {
        hits = Physics.RaycastAll(transform.position, transform.forward, rayDistance);

        if (manager.whoIsTurn == 0)
        {
            manager.X.SetActive(true);
            manager.O.SetActive(false);

            manager.whoIsTurnText = "O";
        }
        else
        {
            manager.X.SetActive(false);
            manager.O.SetActive(true);

            manager.whoIsTurnText = "X";
        }

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
        }
        if (hits.Length >= 3)
        {
            if (hits[0].transform.name == hits[1].transform.name && hits[1].transform.name == hits[2].transform.name)
            {
                menu.ActiveCurrentScreen(3);
                manager.results.text = manager.whoIsTurnText;

                if (manager.whoIsTurnText == "X") mManager.ScoreX++;
                else mManager.ScoreO++;
            }
        }


        if (manager.checkBox == mManager.formatNumber)
        {
            menu.ActiveCurrentScreen(3);
            manager.results.text = ("DRAW");
        }
    }
}