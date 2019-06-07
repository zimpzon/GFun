using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum QuestId {
    ReaperAnnoyed,
    ReaperAnnoyedMultipleInOneRun,
    ReachFloor3World1,
    ReachFloor6World1,
    ReachFloor10World1,
    ReachFloor15World1,
    ReachFloor3World2,
    ReachFloor6World2,
    ReachFloor10World2,
    ReachFloor15World2,
    DiscoverAnySecretAchievement,
    Discover10SecretFloorTiles,
    KillBossWorld1,
    KillBossWorld1With1HpLeft,
    KillBossWorld1NoDamageInRun,
    ReachLevel5,
    ReachLevel10,
    ReachLevel15,
    DestroyGolemNest,
};

public class Quest
{
    public QuestId Id;
    public Func<QuestProgress, string> GetDisplayText;
    public Func<QuestProgress, string> GetRewardText;
    public Func<QuestProgress, bool> CheckCurrentCompletionState; // A little cryptic to distinguish it from the list of completed quests. This only checks RIGHT now, not persisted settings.
    public Func<QuestProgress, bool> IsVisibleToPlayer = (QuestProgress _) => true;
    public string Description;
    public Action ActivationFunc; // If not null will show an activation button in UI
    public Action ApplyReward = () => { };
    public int Level;

    //public static Quest Create(Func<QuestProgress, string> GetDisplayText, string description, Func<QuestProgress, string> GetRewardText, Func<QuestProgress, bool> IsCompleted, Action ApplyReward, int Level)
    //{
    //    var q = new Quest
    //    {
    //        GetDisplayText = GetDisplayText,
    //        Description = description,
    //        GetRewardText = GetRewardText,
    //        IsCompleted = IsCompleted,
    //        ApplyReward = ApplyReward,
    //        Level = Level
    //    };
    //    return q;
    //}
}

[Serializable]
public class QuestProgress
{
    public class CurrentRunStats
    {
        public int ReaperAnnoyed;
    }

    public class PersistedStats
    {
        public int ReaperAnnoyed;
    }

    public List<Quest> Quests = new List<Quest>();
    public List<QuestId> CollectedQuests = new List<QuestId>();
    public List<QuestId> CompletedQuests = new List<QuestId>();

    public PersistedStats persistedStats = new PersistedStats();
    public CurrentRunStats currentRunStats = new CurrentRunStats();

    public int NumberOfDeaths;
    public Dictionary<EnemyId, int> EnemyKills = new Dictionary<EnemyId, int>();
    public int ReaperAnnoyedTotal;
    public bool GolemNestDestroyed;
    public bool ReachedGolemKingWithNoDamage;
    readonly Dictionary<QuestId, Quest> questLookup_ = new Dictionary<QuestId, Quest>();

    public int CountQuestsPendingCollection() => Quests.Where(q => IsCompleted(q.Id) && !IsCollected(q.Id)).Count();

    public void BeginQuestTracking()
    {
        GameEvents.OnQuestEvent += GameEvents_OnQuestEvent;
    }

    public void ApplyAllRewards()
    {
        for (int i = 0; i < Quests.Count; ++i)
        {
            var quest = Quests[i];
            if (IsCompleted(quest.Id) && IsCollected(quest.Id))
                quest.ApplyReward?.Invoke();
        }
    }

    void TestCompletion(QuestId id)
    {
        var quest = questLookup_[id];
        bool wasComplete = IsCompleted(id);
        if (!wasComplete && quest.CheckCurrentCompletionState(this))
        {
            CompletedQuests.Add(id);
            GameEvents.RaiseQuestCompletedEvent(quest);
        }
    }

    public bool IsCompleted(QuestId id) => CompletedQuests.Contains(id);
    public bool IsCollected(QuestId id) => CollectedQuests.Contains(id);
    public void CollectQuest(QuestId id) => CollectedQuests.Add(id);

    private void GameEvents_OnQuestEvent(QuestEvent evt)
    {
        Debug.Log("QuestEvent: " + evt);

        switch(evt)
        {
            case QuestEvent.RunStarted:
                currentRunStats = new CurrentRunStats();
                break;

            case QuestEvent.ReaperAnnoyed:
                currentRunStats.ReaperAnnoyed++;
                persistedStats.ReaperAnnoyed++;
                TestCompletion(QuestId.ReaperAnnoyed);
                TestCompletion(QuestId.ReaperAnnoyedMultipleInOneRun);
                break;

            // Instead: On GolemKingKilled, if damage taken == 0 then completed.
            //case QuestEvent.ReachedGolemKingWithNoDamage:
            //    break;

            case QuestEvent.GolemNestDestroyed:
                GolemNestDestroyed = true;
                TestCompletion(QuestId.DestroyGolemNest);
                break;

            default:
                DebugLinesScript.Show("Unknown quest event: " + evt.ToString(), Time.time);
                break;
        }
    }

    static string Prog(int current, int required) => $"(<color=#ffffff>{(current > required ? required : current)}</color>/<color=#ffffff>{required}</color>)";

    public QuestProgress()
    {
        CreateQuests();
        foreach (var quest in Quests)
            questLookup_[quest.Id] = quest;
    }

    public void PrintQuestDebugOutput()
    {
        foreach(var q in Quests)
        {
            string line = $"{q.GetDisplayText(this)}: isCompleted: {IsCompleted(q.Id)}, isCollected: {IsCollected(q.Id)}";
            DebugLinesScript.Show(line, Time.time);
            Debug.Log(line);
        }
    }

    void CreateQuests()
    {
        Quests.Clear();

        Quests.Add(new Quest
        {
            Id = QuestId.ReaperAnnoyed,
            GetDisplayText = (qp) => $"Annoy The Reaper In Reapers Hideout",
            Description = "A punishment can sometimes be turned into a benefit. But be careful not to overdo it!",
            GetRewardText = (qp) => $"+10 Starting Gold",
            CheckCurrentCompletionState = (qp) => currentRunStats.ReaperAnnoyed >= 1,
            ApplyReward = () => { CurrentRunData.Instance.Coins += 10; },
            Level = 1,
        });

        Quests.Add(new Quest
        {
            Id = QuestId.ReaperAnnoyedMultipleInOneRun,
            GetDisplayText = (qp) => $"Annoy The Reaper 3 Times In One Run", // count
            Description = "Time To Test His Limits!",
            GetRewardText = (qp) => $"+10 Starting Gold",
            CheckCurrentCompletionState = (qp) => currentRunStats.ReaperAnnoyed >= 3, // count
            ApplyReward = () => { CurrentRunData.Instance.Coins += 10; },
            IsVisibleToPlayer = (qp) => IsCompleted(QuestId.ReaperAnnoyed),
            Level = 2,
        });

        Quests.Add(new Quest
        {
            Id = QuestId.DestroyGolemNest,
            GetDisplayText = (qp) => $"Destroy A Golem Nest",
            Description = "We have located a golem nest. Let me know when you are ready to eliminate this menace and I will open a direct portal.",
            GetRewardText = (qp) => $"Achievement",
            CheckCurrentCompletionState = (qp) => qp.GolemNestDestroyed == true,
            IsVisibleToPlayer = (qp) => IsCompleted(QuestId.ReaperAnnoyed),
            Level = 5,
            ActivationFunc = () => { },
        });
    }
}
