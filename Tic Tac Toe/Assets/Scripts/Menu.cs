using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;


public class Menu : MonoBehaviour
{
    public GameObject[] scenes;
    public Animator animator;
    MenuManager menuManager;
    void Start()
    {
        menuManager = GameObject.FindGameObjectWithTag("MenuManager").GetComponent<MenuManager>();
    }

    public void SceneLoad(int number)
    {
        if (number == 0) Destroy(GameObject.FindGameObjectWithTag("MenuManager"));

        Destroy(GameObject.FindGameObjectWithTag("GameManager"));
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
    public void SoundEffect(AudioClip audioClip)
    {
        gameObject.transform.GetComponent<AudioSource>().PlayOneShot(audioClip);
    }
    public void SelectFormat(bool format)
    {
        menuManager.format3x3 = format;

        if (format) menuManager.formatNumber = 9;
        else menuManager.formatNumber = 16;
    }
    public void Quit()
    {
        Application.Quit();
    }
}
