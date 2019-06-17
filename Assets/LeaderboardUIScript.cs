using TMPro;
using UnityEngine;

public class LeaderboardUIScript : MonoBehaviour
{
    public TextMeshProUGUI TextRank;
    public TextMeshProUGUI TextName;
    public TextMeshProUGUI TextScore;

    public void SetText(string rank, string name, string score)
    {
        TextRank.text = rank;
        TextName.text = name;
        TextScore.text = score;
    }
}
