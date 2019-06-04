using UnityEngine;

public class QuestGiverScript : MonoBehaviour
{
    public Canvas QuestCanvas;
    public GameObject QuestContentParent;
    public GameObject QuestPrefab;

    SpriteAnimator_Single anim_;
    Transform transform_;

    public void OnTalk()
    {
        QuestCanvas.gameObject.SetActive(true);
        for (int i = 0; i < QuestContentParent.transform.childCount; ++i)
            DestroyImmediate(QuestContentParent.transform.GetChild(i));

        var qp = GameProgressData.CurrentProgress.QuestProgress;
        for (int i = 0; i < qp.Quests.Count; ++i)
        {
            var quest = qp.Quests[i];
            var uiQuest = Instantiate(QuestPrefab).GetComponent<QuestUIScript>();
            uiQuest.SetQuest(quest, qp);
            uiQuest.transform.SetParent(QuestContentParent.transform);
            uiQuest.transform.localPosition = new Vector3(5, i * 50, 0);
        }
    }

    void Start()
    {
        anim_ = GetComponent<SpriteAnimator_Single>();
        transform_ = transform;
    }

    void Update()
    {
        Vector3 directionToPlayer = AiBlackboard.Instance.PlayerPosition - transform_.position;
        anim_.UpdateAnimation(directionToPlayer);
    }
}
