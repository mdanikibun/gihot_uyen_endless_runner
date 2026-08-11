using UnityEngine;
using TMPro;

public class LeaderboardRowUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TMP_Text rankText;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text distanceText;
    [SerializeField] TMP_Text scoreText;

    public void Setup(int rank, string playerName, float distance, int score) {
        rankText.text = "#" + rank;
        nameText.text = playerName;
        distanceText.text = Mathf.RoundToInt(distance) + "m";
        scoreText.text = score.ToString();
    }
}
