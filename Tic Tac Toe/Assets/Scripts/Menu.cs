using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public GameObject[] scenes;
    public Animator animator;

    void Start()
    {

    }


    public void SceneLoad(int number)
    {
        SceneManager.LoadScene(number);
    }
    public void ActiveCurrentScreen(int currentSceneNumber)
    {
        for (int i = 0; i < scenes.Length; i++)
        {
            scenes[i].SetActive(false);
        }
        scenes[currentSceneNumber].SetActive(true);
    }
    public void Loading()
    {
        animator.Play("StartGame");
    }
    public void Returning()
    {
        animator.Play("ReturnMenu");
    }
    public void Quit()
    {
        Application.Quit();
    }
}
