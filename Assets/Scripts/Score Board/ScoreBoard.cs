using System;
using UnityEngine;
using TMPro;

public class ScoreBoard : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI score_X, score_O, finalResult;
    private string gameMode;

    private void Start()
    {
        gameMode = PlayerPrefs.GetString("Format");
    }
    public void UpdateScoreTable()
    {
        score_X.text = Convert.ToString(PlayerPrefs.GetInt("ScoreX"));
        score_O.text = Convert.ToString(PlayerPrefs.GetInt("ScoreO"));
    }
    public void ResultScreen(GameManager.GameResult gameResult)
    {
        UpdateScoreTable();

        if (gameMode == "infinity")
            if (gameResult == GameManager.GameResult.draw)
            {
                finalResult.text =
                    (PlayerPrefs.GetInt("ScoreX") == PlayerPrefs.GetInt("ScoreO")) ? "Draw!" :
                    (PlayerPrefs.GetInt("ScoreX") == PlayerPrefs.GetInt("ScoreO")) ? "x Wins!" : "O Wins!";
                gameOverPanel.SetActive(true);
                return;
            }
            else
                return;

        if (gameResult == GameManager.GameResult.X_win)
            finalResult.text = "X Wins!";
        else if (gameResult == GameManager.GameResult.O_win)
            finalResult.text = "O Wins!";
        else if (gameResult == GameManager.GameResult.draw)
            finalResult.text = "Draw!";

        gameOverPanel.SetActive(true);
    }
}
