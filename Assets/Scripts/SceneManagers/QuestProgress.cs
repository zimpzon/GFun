using System;
using System.Collections.Generic;
using UnityEngine;

public enum QuestId { ReaperAnnoyed, };

public class Quest
{
    public QuestId Id;
    public Func<QuestProgress, string> GetDisplayText;
    public Func<QuestProgress, string> GetRewardText;
    public Func<QuestProgress, bool> IsCompleted;
    public string Description;
    public Action ActivationFunc;
    public Action ApplyReward;
    public int Level;

    public static Quest Create(Func<QuestProgress, string> GetDisplayText, string description, Func<QuestProgress, string> GetRewardText, Func<QuestProgress, bool> IsCompleted, Action ApplyReward, int Level)
    {
        var q = new Quest
        {
            GetDisplayText = GetDisplayText,
            Description = description,
            GetRewardText = GetRewardText,
            IsCompleted = IsCompleted,
            ApplyReward = ApplyReward,
            Level = Level
        };
        return q;
    }
}

[Serializable]
public class QuestProgress
{
    public List<Quest> Quests = new List<Quest>();
    public List<QuestId> CollectedQuests = new List<QuestId>();

    public int NumberOfDeaths;
    public Dictionary<EnemyId, int> EnemyKills = new Dictionary<EnemyId, int>();
    public int ReaperAnnoyed;

    public void BeginQuestTracking()
    {
        GameEvents.OnQuestEvent += GameEvents_OnQuestEvent;
    }

    void UpdateQuest(Quest quest, Action update)
    {
        bool wasComplete = quest.IsCompleted(this);
        update();
        if (!wasComplete && quest.IsCompleted(this))
            GameEvents.RaiseQuestCompletedEvent(quest);
    }

    public bool IsCollected(Quest quest) => CollectedQuests.Contains(quest.Id);
    public void CollectQuest(Quest quest) => CollectedQuests.Add(quest.Id);

    private void GameEvents_OnQuestEvent(QuestEvent evt)
    {
        switch(evt)
        {
            case QuestEvent.ReaperAnnoyed: UpdateQuest(QReaperAnnoyed, () => ReaperAnnoyed++); break;
            case QuestEvent.ReachedGolemKingWithNoDamage: break;
            default: DebugLinesScript.Show("Unknown quest event: " + evt.ToString(), Time.time); break;
        }
    }

    static string Prog(int current, int required) => $"(<color=#ffffff>{current}</color>/<color=#ffffff>{required}</color>)";

    public QuestProgress()
    {
        Quests.Add(QReaperAnnoyed);
        Quests.Add(QReaperAnnoyed2);
        Quests.Add(Q3);
    }

    Quest QReaperAnnoyed = new Quest
    {
        Id = QuestId.ReaperAnnoyed,
        GetDisplayText = (qp) => $"Annoy The Reaper In Reapers Hideout {Prog(qp.ReaperAnnoyed, 1)}",
        Description = "A punishment can sometimes be turned into a benefit. But be careful not to overdo it!",
        GetRewardText = (qp) => $"+10 Starting Gold",
        IsCompleted = (qp) => qp.ReaperAnnoyed >= 1,
        ApplyReward = () => { CurrentRunData.Instance.Coins += 10; },
        Level = 1,
    };

    Quest QReaperAnnoyed2 = new Quest
    {
        Id = QuestId.ReaperAnnoyed,
        GetDisplayText = (qp) => $"Destroy A Golem Nest {Prog(qp.ReaperAnnoyed, 1)}",
        Description = "We have located a golem nest. Let me know when you are ready to eliminate this menace and I will open a direct portal.",
        GetRewardText = (qp) => $"Achievement",
        IsCompleted = (qp) => qp.ReaperAnnoyed >= 1,
        Level = 1,
        ActivationFunc = () => { },
    };

    Quest Q3 = new Quest
    {
        Id = QuestId.ReaperAnnoyed,
        GetDisplayText = (qp) => $"Destroy A Golem Nest {Prog(1, 1)}",
        Description = "We have located a golem nest. Let me know when you are ready to eliminate this menace and I will open a direct portal.",
        GetRewardText = (qp) => $"Achievement",
        IsCompleted = (qp) => true,
        Level = 1,
        ActivationFunc = () => { },
    };
}
