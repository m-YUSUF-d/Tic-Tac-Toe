using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class Cell : MonoBehaviour
{
    //public UnityEvent onClickCell;

    [SerializeField] private Sprite sprite_X, sprite_O, defaultSprite;

    private Board board;
    private GameManager gm;
    private void Start()
    {
        gm = FindFirstObjectByType<GameManager>();
        board = FindAnyObjectByType<Board>();
        GetComponent<Button>().onClick.AddListener(() => TableButtonAction());
    }

    public void TableButtonAction()
    {
        GameObject clickedObject = EventSystem.current.currentSelectedGameObject;
        clickedObject.GetComponent<Button>().enabled = false;

        //change button image to whose turn it is
        Image img = clickedObject.GetComponent<Image>();
        if (GameManager.isTurn_X == true) img.sprite = sprite_X; else img.sprite = sprite_O;    //true->X , false->O

        //onClickCell.Invoke(); do it later

        board.SetBoard(int.Parse(clickedObject.name), (GameManager.isTurn_X == true) ? "X" : "O");
        board.audioSource.PlayOneShot(board.clickSound);

        gm.GameStateCheck();
        gm.ChangeTurn();
    }
    public void ResetCell()
    {
        GetComponent<Button>().enabled = true;
        GetComponent<Image>().sprite = defaultSprite;
    }
}
