using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;

public class GameManager : MonoBehaviour
{
    public int whoIsTurn;
    public int scoreX, scoreO;
    public GameObject X, O;
    public TextMeshProUGUI textX, textO;
    void Start()
    {
        whoIsTurn = 0;
        DontDestroyOnLoad(gameObject);

        textX.text = scoreX.ToString();
        textO.text = scoreO.ToString();
    }

    void Update()
    {
        if (whoIsTurn == 0)
        {
            X.SetActive(true);
            O.SetActive(false);
        }
        else
        {
            X.SetActive(false);
            O.SetActive(true);
        }
    }
}
