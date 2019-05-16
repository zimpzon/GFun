using UnityEngine;

public static class GameEvents
{
    public delegate void NoParamEventHandler();
    public delegate void EnemyEventHandler(IEnemy enemy);
    public delegate void EnemyAndPositionEventHandler(IEnemy enemy, Vector3 position);
    public delegate void AutoPickUpEventHandler(AutoPickUpType type, int value, Vector3 position);

    public static event EnemyEventHandler OnPlayerKilled;
    public static event EnemyEventHandler OnPlayerDamaged;
    public static event EnemyAndPositionEventHandler OnEnemyKilled;
    public static event EnemyAndPositionEventHandler OnEnemySpawned;
    public static event AutoPickUpEventHandler OnAutoPickUp;
    public static event NoParamEventHandler OnPlayerEnteredPortal;

    public static void RaisePlayerKilled(IEnemy enemy) => OnPlayerKilled?.Invoke(enemy);
    public static void RaisePlayerDamaged(IEnemy enemy) => OnPlayerDamaged?.Invoke(enemy);
    public static void RaiseEnemyKilled(IEnemy enemy, Vector3 position) => OnEnemyKilled?.Invoke(enemy, position);
    public static void RaiseEnemySpawned(IEnemy enemy, Vector3 position) => OnEnemySpawned?.Invoke(enemy, position);
    public static void RaiseAutoPickUp(AutoPickUpType type, int value, Vector3 position) => OnAutoPickUp?.Invoke(type, value, position);
    public static void RaisePlayerEnteredPortal() => OnPlayerEnteredPortal?.Invoke();

    public static void ClearListeners()
    {
        OnPlayerKilled = null;
        OnPlayerDamaged = null;
        OnEnemyKilled = null;
        OnEnemySpawned = null;
        OnAutoPickUp = null;
        OnPlayerEnteredPortal = null;
    }
}
