using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameMenu : MonoBehaviour
{
    public GameObject[] uiElements;

    [SerializeField] private AudioClip clickSound;
    private AudioSource audioSource;

    private void Awake()
    {
        SetActiveUIElement("0");
        audioSource = GetComponent<AudioSource>();

        //tüm butonlara aynı click sesini ekler
        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Button btn in buttons)
            btn.onClick.AddListener(() => PlayClickSound());
    }
    private void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
            audioSource.PlayOneShot(clickSound);
    }


    //methods for canvas buttons
    public void SetActiveUIElement(string sectionName)
    {
        for (int i = 0; i < uiElements.Length; i++)
            uiElements[i].SetActive(uiElements[i].name == sectionName);

        if (sectionName == "0") uiElements[0].SetActive(true);
    }
    public void LoadScene(int number)   //load scene with index number
    {
        SceneManager.LoadScene(number);
    }
    public void Quit()  //quit the application
    {
        Application.Quit();
    }
}
