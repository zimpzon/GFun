using TMPro;
using UnityEngine;

public class QuestUIScript : MonoBehaviour
{
    public TextMeshProUGUI QuestText;
    public TextMeshProUGUI RewardText;

    public void SetQuest(Quest quest, QuestProgress progress)
    {
        QuestText.text = quest.GetDisplayText(progress);
        RewardText.text = quest.GetRewardText(progress);

        bool isCollected = progress.IsCollected(quest);
        if (!isCollected)
        {
        }
    }
}
