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
        completionPending_ = false;
        if (activateQuestOnButtonPress_)
        {
            CampScript.Instance.ActivateQuest(quest_.Id);
        }
        else
        {
            // Collecting reward
            progress_.CollectQuest(quest_.Id);
            SetQuest(quest_, progress_);

            QuestGiverScript.Instance.CompleteParticles.transform.position = this.transform.position;
            QuestGiverScript.Instance.CompleteParticles.Emit(21);
            AudioManager.Instance.PlaySfxClip(QuestGiverScript.Instance.CompleteSound, 5, 0, 1.0f);
            AudioManager.Instance.PlaySfxClip(QuestGiverScript.Instance.CompleteSound, 5, 0, 0.90f);
            AudioManager.Instance.PlaySfxClip(QuestGiverScript.Instance.CompleteSound, 5, 0, 1.1f);

            GameProgressData.SaveProgress();
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

        bool isCompleted = progress.IsCompleted(quest.Id);
        bool isCollected = progress.IsCollected(quest.Id);
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
            QuestText.text += "  <color=#00ff00>(COMPLETE)</color>";
        }
        else if (isCompleted && !isCollected)
        {
            // Quest is completed but not yet collected
            backgroundImage.color = QuestGiverScript.Instance.CompletedColor;
            QuestText.text += "  <color=#00ff00>(COMPLETE)</color>";
            buttontext.text = "Complete";
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
