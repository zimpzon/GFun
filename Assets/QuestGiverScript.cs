using System.Linq;
using TMPro;
using UnityEngine;

public class QuestGiverScript : MonoBehaviour
{
    public static QuestGiverScript Instance;

    public Color CollectButtonColor = Color.green;
    public Color ActivateButtonColor = Color.magenta;
    public Color QuestColor = Color.yellow;
    public Color AchievementColor = Color.cyan;
    public Color CompletedColor = Color.green;
    public Color CollectedColor = Color.grey;

    public Canvas QuestCanvas;
    public GameObject QuestContentParent;
    public GameObject QuestPrefab;
    public TextMeshPro TextNotify;
    public ParticleSystem CompleteParticles;
    public AudioClip CompleteSound;
    public AudioClip ShowSound;
    public AudioClip CloseSound;

    SpriteAnimator_Single anim_;
    Transform transform_;

    void UpdateNotifyText()
    {
        int pendingCount = GameProgressData.CurrentProgress.QuestProgress.CountQuestsPendingCollection();
        TextNotify.text = pendingCount == 0 ? "!" : "?";
    }

    public void Close()
    {
        UpdateNotifyText();
        QuestCanvas.gameObject.SetActive(false);
        HumanPlayerController.Disabled = false;
        AudioManager.Instance.PlaySfxClip(CloseSound, 1);
        CampScript.Instance.ShowPlayerName(true);
    }

    public void OnTalk()
    {
        CampScript.Instance.ShowPlayerName(false);
        HumanPlayerController.Disabled = true;
        QuestCanvas.gameObject.SetActive(true);

        while(QuestContentParent.transform.childCount > 0)
            DestroyImmediate(QuestContentParent.transform.GetChild(0).gameObject);

        var qp = GameProgressData.CurrentProgress.QuestProgress;
        var collectedQuests = qp.Quests.Where(q => qp.IsCompleted(q.Id) && qp.IsCollected(q.Id)).OrderBy(q => q.GetDisplayText(qp)).ToList();
        var completedQuests = qp.Quests.Where(q => qp.IsCompleted(q.Id) && !qp.IsCollected(q.Id)).OrderBy(q => q.GetDisplayText(qp)).ToList();
        var activeQuests = qp.Quests.Where(q => !qp.IsCompleted(q.Id)).OrderBy(q => q.GetDisplayText(qp)).ToList();

        var sortedQuests = completedQuests.Concat(activeQuests).Concat(collectedQuests).ToList();
        for (int i = 0; i < sortedQuests.Count; ++i)
        {
            var quest = sortedQuests[i];
            if (!quest.IsVisibleToPlayer(qp))
                continue;

            var uiQuest = Instantiate(QuestPrefab).GetComponent<QuestUIScript>();
            uiQuest.SetQuest(quest, qp);
            uiQuest.transform.SetParent(QuestContentParent.transform);
            uiQuest.transform.localScale = Vector3.one;
            uiQuest.transform.localPosition = Vector3.zero;
        }

        AudioManager.Instance.PlaySfxClip(ShowSound, 1);
    }

    void Start()
    {
        Instance = this;
        anim_ = GetComponent<SpriteAnimator_Single>();
        transform_ = transform;
        UpdateNotifyText();
        QuestCanvas.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && QuestCanvas.gameObject.activeInHierarchy)
            Close();

        Vector3 directionToPlayer = AiBlackboard.Instance.PlayerPosition - transform_.position;
        anim_.UpdateAnimation(directionToPlayer);
    }
}
