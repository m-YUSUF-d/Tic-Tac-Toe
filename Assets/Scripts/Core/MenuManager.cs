using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject[] uiElements;

    [SerializeField] private AudioClip clickSound;
    private AudioSource audioSource;

    private void Start()
    {
        SetActiveUIElement("0");
        audioSource = GetComponent<AudioSource>();

        //oyun başlamadan önce tüm skorları 0'a ayarlar
        PlayerPrefs.SetInt("ScoreX", 0);
        PlayerPrefs.SetInt("ScoreO", 0);

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
    public void SetGameFormat(string format)   //set game format from the menu
    {
        PlayerPrefs.SetString("Format", format);
    }
    public void Quit()  //quit the application
    {
        Application.Quit();
    }
}
