using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    public AudioClip clickSound;
    [HideInInspector] public AudioSource audioSource;

    [SerializeField] private string[] gameBoard;
    [SerializeField] private int moveNum = 0, maxMoveNum;
    private List<int[]> winCombinations = new List<int[]>();

    [SerializeField] private float cellSize_3x3, cellSize_4x4;
    [SerializeField] private Button[] buttons;
    [SerializeField] private Sprite sprite_X, sprite_O;
    [SerializeField] private GameObject boardCell, boardLock;

    private Lock Lock;
    private GridLayoutGroup grid;

    private void Start()
    {
        grid = GetComponent<GridLayoutGroup>();
        audioSource = GetComponent<AudioSource>();

        CreateBoard(PlayerPrefs.GetString("Format"));
    }

    private void CreateBoard(string gameMode)
    {
        buttons = null;

        //find game format number to a int variable
        int gameFormatNum = (gameMode == "3x3") ? 3 : 4;
        buttons = new Button[(int)Mathf.Pow(gameFormatNum, 2)];
        maxMoveNum = (int)Mathf.Pow(gameFormatNum, 2);

        //set cell size according to game format
        grid.cellSize = (gameFormatNum == 3) ? new Vector2(cellSize_3x3, cellSize_3x3) : new Vector2(cellSize_4x4, cellSize_4x4);

        if (gameMode == "4x4")//create lock cell for 4x4 and get component of Lock script
            Lock = Instantiate(boardLock, transform.parent).GetComponent<Lock>();

        for (int i = 0; i < Mathf.Pow(gameFormatNum, 2); i++)// create buttons into grid and add them to the buttons array
        {
            buttons[i] = Instantiate(boardCell, grid.transform).GetComponent<Button>();
            buttons[i].name = i.ToString();
        }

        CreateCombos(gameFormatNum);
        gameBoard = new string[(int)Mathf.Pow(gameFormatNum, 2)];
    }
    private void CreateCombos(int boardSize)
    {
        int winLength = 3;
        for (int row = 0; row < boardSize; row++)
        {
            for (int col = 0; col <= boardSize - winLength; col++)
            {
                int[] combo = new int[winLength];
                for (int k = 0; k < winLength; k++)
                    combo[k] = row * boardSize + (col + k);

                winCombinations.Add(combo);
            }
        }

        // SÜTUNLAR
        for (int col = 0; col < boardSize; col++)
        {
            for (int row = 0; row <= boardSize - winLength; row++)
            {
                int[] combo = new int[winLength];
                for (int k = 0; k < winLength; k++)
                    combo[k] = (row + k) * boardSize + col;

                winCombinations.Add(combo);
            }
        }

        // ÇAPRAZ
        for (int row = 0; row <= boardSize - winLength; row++)
        {
            for (int col = 0; col <= boardSize - winLength; col++)
            {
                int[] combo = new int[winLength];
                for (int k = 0; k < winLength; k++)
                    combo[k] = (row + k) * boardSize + (col + k);

                winCombinations.Add(combo);
            }
        }

        // ÇAPRAZ
        for (int row = 0; row <= boardSize - winLength; row++)
        {
            for (int col = winLength - 1; col < boardSize; col++)
            {
                int[] combo = new int[winLength];
                for (int k = 0; k < winLength; k++)
                    combo[k] = (row + k) * boardSize + (col - k);

                winCombinations.Add(combo);
            }
        }
    }


    public void SetBoard(int index, string symbol)
    {
        gameBoard[index] = symbol;
        moveNum++;
    }
    public GameManager.GameResult WinCheck()
    {
        string gameMode = PlayerPrefs.GetString("Format");
        if (gameMode == "4x4" && Lock.number > 0)
            Unlock();

        foreach (var combo in winCombinations)
        {
            string a = gameBoard[combo[0]];
            string b = gameBoard[combo[1]];
            string c = gameBoard[combo[2]];

            if (!string.IsNullOrEmpty(a) && a == b && b == c)
            {
                if (gameMode == "infinity")
                {
                    gameBoard[combo[0]] = null;
                    gameBoard[combo[1]] = null;
                    gameBoard[combo[2]] = null;

                    moveNum -= 3;

                    buttons[combo[0]].GetComponent<Cell>().ResetCell();
                    buttons[combo[1]].GetComponent<Cell>().ResetCell();
                    buttons[combo[2]].GetComponent<Cell>().ResetCell();
                }
                if (GameManager.isTurn_X)
                {
                    PlayerPrefs.SetInt("ScoreX", PlayerPrefs.GetInt("ScoreX") + 1);
                    return GameManager.GameResult.X_win;
                }
                else
                {
                    PlayerPrefs.SetInt("ScoreO", PlayerPrefs.GetInt("ScoreO") + 1);
                    return GameManager.GameResult.O_win;
                }
            }
        }
        if (moveNum >= maxMoveNum) return GameManager.GameResult.draw;

        else return GameManager.GameResult.none;
    }
    private void Unlock()
    {
        Lock.LockCheck();
    }
}

