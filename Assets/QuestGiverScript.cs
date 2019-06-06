using System.Linq;
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

    SpriteAnimator_Single anim_;
    Transform transform_;

    public void OnClose()
    {
        QuestCanvas.gameObject.SetActive(false);
        HumanPlayerController.Disabled = false;
    }

    public void OnTalk()
    {
        HumanPlayerController.Disabled = true;
        QuestCanvas.gameObject.SetActive(true);

        for (int i = 0; i < QuestContentParent.transform.childCount; ++i)
            DestroyImmediate(QuestContentParent.transform.GetChild(i).gameObject);

        var qp = GameProgressData.CurrentProgress.QuestProgress;
        var collectedQuests = qp.Quests.Where(q => q.IsCompleted(qp) && qp.IsCollected(q)).OrderBy(q => q.GetDisplayText(qp));
        var completedQuests = qp.Quests.Where(q => q.IsCompleted(qp) && !qp.IsCollected(q)).OrderBy(q => q.GetDisplayText(qp));
        var activeQuests = qp.Quests.Where(q => !q.IsCompleted(qp)).OrderBy(q => q.GetDisplayText(qp));

        var sortedQuests = activeQuests.Concat(completedQuests).Concat(collectedQuests).ToList();
        for (int i = 0; i < sortedQuests.Count; ++i)
        {
            var quest = qp.Quests[i];
            var uiQuest = Instantiate(QuestPrefab).GetComponent<QuestUIScript>();
            uiQuest.SetQuest(quest, qp);
            uiQuest.transform.SetParent(QuestContentParent.transform);
            uiQuest.transform.SetAsLastSibling();
        }
    }

    void Start()
    {
        Instance = this;
        anim_ = GetComponent<SpriteAnimator_Single>();
        transform_ = transform;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && QuestCanvas.gameObject.activeInHierarchy)
            OnClose();

        Vector3 directionToPlayer = AiBlackboard.Instance.PlayerPosition - transform_.position;
        anim_.UpdateAnimation(directionToPlayer);
    }
}
