using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    public GameObject[] X_O;
    public GameManager manager;

    private void Start()
    {
        manager = FindObjectOfType(typeof(GameManager)).GetComponent<GameManager>();
    }
    public void Create()
    {
        manager = FindObjectOfType(typeof(GameManager)).GetComponent<GameManager>();
        Instantiate(X_O[manager.whoIsTurn], transform.position, Quaternion.identity);

        if (manager.whoIsTurn == 0)
        {
            manager.whoIsTurn = 1;
        }
        else
        {
            manager.whoIsTurn = 0;

        }

        Destroy(gameObject);
    }
}
