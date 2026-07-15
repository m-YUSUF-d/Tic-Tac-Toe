using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject icons_X, icon_O;

    public static bool isTurn_X { get; private set; }
    private Board board;
    private ScoreBoard scoreBoard;

    public enum GameResult
    {
        X_win,
        O_win,
        draw,
        none
    }

    private void Start()
    {
        board = FindFirstObjectByType<Board>();

        if (PlayerPrefs.GetString("Format") == "infinity")
        {
            PlayerPrefs.SetInt("ScoreX", 0);
            PlayerPrefs.SetInt("ScoreO", 0);
        }

        scoreBoard = FindFirstObjectByType<ScoreBoard>();
        scoreBoard.UpdateScoreTable();
        ChangeTurn(true);
    }
    public void GameStateCheck()
    {
        GameResult result = board.WinCheck();
        if (result == GameResult.none) return;
        scoreBoard.ResultScreen(result);
    }
    public void ChangeTurn(bool initalCall = false)
    {
        isTurn_X = (initalCall == true) ? true : !isTurn_X;
        icons_X.SetActive(isTurn_X);
        icon_O.SetActive(!isTurn_X);
    }
}
