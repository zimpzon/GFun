using GFun;
using UnityEngine;

public enum QuestEvent { ReaperAnnoyed, ReachedGolemKingWithNoDamage };

public static class GameEvents
{
    public delegate void QuestEventHandler(QuestEvent evt);
    public delegate void QuestCompletedEventHandler(Quest quest);
    public delegate void NoParamEventHandler();
    public delegate void EnemyEventHandler(IEnemy enemy);
    public delegate void AmmoChangeEventHandler(AmmoType ammoType, int change);
    public delegate void EnemyAndPositionEventHandler(IEnemy enemy, Vector3 position);
    public delegate void AutoPickUpEventHandler(AutoPickUpType type, int value, Vector3 position);

    public static event QuestEventHandler OnQuestEvent;
    public static event QuestCompletedEventHandler OnQuestCompleted;
    public static event EnemyEventHandler OnPlayerKilled;
    public static event EnemyEventHandler OnPlayerDamaged;
    public static event AmmoChangeEventHandler OnAmmoChanged;
    public static event EnemyAndPositionEventHandler OnEnemyKilled;
    public static event EnemyAndPositionEventHandler OnEnemySpawned;
    public static event AutoPickUpEventHandler OnAutoPickUp;
    public static event NoParamEventHandler OnPlayerEnteredPortal;
    public static event NoParamEventHandler OnGolemKingCallingForHelp;
    public static event NoParamEventHandler OnAllEnemiesKilled;

    public static void RaiseQuestEvent(QuestEvent evt) => OnQuestEvent?.Invoke(evt);
    public static void RaiseQuestCompletedEvent(Quest quest) => OnQuestCompleted?.Invoke(quest);
    public static void RaisePlayerKilled(IEnemy enemy) => OnPlayerKilled?.Invoke(enemy);
    public static void RaisePlayerDamaged(IEnemy enemy) => OnPlayerDamaged?.Invoke(enemy);
    public static void RaiseAmmoChanged(AmmoType ammoType, int change) => OnAmmoChanged?.Invoke(ammoType, change);
    public static void RaiseEnemyKilled(IEnemy enemy, Vector3 position) => OnEnemyKilled?.Invoke(enemy, position);
    public static void RaiseEnemySpawned(IEnemy enemy, Vector3 position) => OnEnemySpawned?.Invoke(enemy, position);
    public static void RaiseAutoPickUp(AutoPickUpType type, int value, Vector3 position) => OnAutoPickUp?.Invoke(type, value, position);
    public static void RaisePlayerEnteredPortal() => OnPlayerEnteredPortal?.Invoke();
    public static void RaiseGolemKingCallForHelp() => OnGolemKingCallingForHelp?.Invoke();
    public static void RaiseAllEnemieKiled() => OnAllEnemiesKilled?.Invoke();

    public static void ClearListeners()
    {
        OnPlayerKilled = null;
        OnPlayerDamaged = null;
        OnAmmoChanged = null;
        OnEnemyKilled = null;
        OnEnemySpawned = null;
        OnAutoPickUp = null;
        OnPlayerEnteredPortal = null;
        OnGolemKingCallingForHelp = null;
        OnAllEnemiesKilled = null;
    }
}
