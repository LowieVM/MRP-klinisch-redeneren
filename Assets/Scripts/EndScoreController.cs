using TMPro;
using UnityEngine;

public class GetEndScore : MonoBehaviour
{
    public TextMeshProUGUI speedScoreText;
    public TextMeshProUGUI totalScoreText;

    private int speedScore = 0;
    private int totalScore = 0;

    public void Start()
    {
        speedScore = PlayerPrefs.GetInt("SpeedScore");

        speedScoreText.text = "Speed of care: " + speedScore;

        totalScore = CalculateTotalScore();

        totalScoreText.text = "Score: " + totalScore;
    }

    private int CalculateTotalScore()
    {
        return speedScore;
    }
}
