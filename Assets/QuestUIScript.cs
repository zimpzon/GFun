using MEC;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestUIScript : MonoBehaviour
{
    public TextMeshProUGUI QuestText;
    public TextMeshProUGUI DescriptionText;
    public TextMeshProUGUI RewardText;
    public GameObject Button;

    Quest quest_;
    QuestProgress progress_;
    bool activateQuestOnButtonPress_;
    bool completionPending_;

    public void OnCollect()
    {
        if (activateQuestOnButtonPress_)
        {
            Debug.Log("Activate " + quest_.GetDisplayText(progress_));
        }
        else
        {
            // Collecting reward
            Debug.Log("Collect " + quest_.GetDisplayText(progress_));
            progress_.CollectQuest(quest_);
            completionPending_ = false;
            SetQuest(quest_, progress_);
        }
    }

    IEnumerator<float> CollectMeCo(Image image)
    {
        Color col = image.color;
        Color.RGBToHSV(col, out float h, out float s, out float v);

        Transform transform = image.transform;
        while (completionPending_)
        {
            v = Mathf.Sin(Time.unscaledTime * 4) * 0.25f + 0.75f;
            image.color = Color.HSVToRGB(h, s, v);
            float factor = Mathf.Clamp01(Mathf.Sin(Time.unscaledTime * 4));
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.unscaledTime * 16) * 10 * factor);
            yield return 0;
        }
    }

    public void SetQuest(Quest quest, QuestProgress progress)
    {
        // Types:

        // Active quest
        // Active quest with activate button
        // Completed quest with collect button

        // Completed and collected quest

        quest_ = quest;
        progress_ = progress;

        bool isCompleted = quest.IsCompleted(progress);
        bool isCollected = progress.IsCollected(quest);
        bool mustBeActivated = quest.ActivationFunc != null;

        QuestText.text = quest.GetDisplayText(progress);
        RewardText.text = quest.GetRewardText(progress);
        DescriptionText.text = quest.Description;

        var buttonImage = Button.GetComponent<Image>();
        var buttontext = Button.GetComponentInChildren<TextMeshProUGUI>();
        var backgroundImage = GetComponent<Image>();

        Button.gameObject.SetActive(false);
        QuestText.color = quest_.ApplyReward == null ? QuestGiverScript.Instance.AchievementColor : QuestGiverScript.Instance.QuestColor;

        if (isCollected)
        {
            // Quest is completed and collected
            backgroundImage.color = QuestGiverScript.Instance.CollectedColor;
            RewardText.text = "Completed";
        }
        else if (isCompleted && !isCollected)
        {
            // Quest is completed but not yet collected
            QuestText.text = "<color=#00ff00>Completed</color>: " + QuestText.text;
            backgroundImage.color = QuestGiverScript.Instance.CompletedColor;
            buttontext.text = "Collect";
            buttonImage.color = QuestGiverScript.Instance.CollectButtonColor;
            Button.gameObject.SetActive(true);

            completionPending_ = true;
            Timing.RunCoroutine(CollectMeCo(buttonImage).CancelWith(this.gameObject));
        }
        else
        {
            // Quest is not completed
            if (mustBeActivated)
            {
                buttontext.text = "I'm ready!";
                buttonImage.color = QuestGiverScript.Instance.ActivateButtonColor;
                activateQuestOnButtonPress_ = true;
                Button.gameObject.SetActive(true);
            }
        }
    }
}
