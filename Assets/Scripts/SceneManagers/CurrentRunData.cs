using System;
using System.Collections.Generic;
using UnityEngine;

public enum World { World1, World2 };
public enum SpecialLocation { None, Shop, BossWorld1, }

[Serializable]
public class CurrentRunData
{
    public const int BossActivationFloor = 5;
    public static CurrentRunData Instance = new CurrentRunData();
    public bool HasPlayerData;

    public WeaponIds CurrentWeapon = WeaponIds.NullWeapon;
    public int BulletAmmo = 999;
    public int ShellAmmo = 999;
    public int ExplosiveAmmo = 999;
    public int ArrowAmmo = 999;

    public int FloorInWorld = 1;
    public int TotalFloor = 1;
    public int StartingDifficulty = 0;
    public World World = World.World1;
    public SpecialLocation SpecialLocation = SpecialLocation.None;

    public string StartingCharacterTag = "Character1";
    public int Coins;
    public int Life;
    public int MaxLife;
    public int PlayerExplosionDamage = 4;
    public float PlayerAttractDistance = 3.0f;

    public int ShopKeeperPokeCount;
    public int ItemsBought;
    public int CoinsCollected;
    public int EnemiesKilled;
    public int XpEarned;

    public int Boss1Attempts;
    public int Boss1Kills;
    public float RunStartTime;
    public float RunEndTime;
    public bool RunEnded;
    public List<string> NewUnlockedCharacters = new List<string>();

    public List<PlayerHealthEvent> HealthEvents = new List<PlayerHealthEvent>();

    public void AddPlayerHealthEvent(int amount, string source)
    {
        var player = PlayableCharacters.GetPlayerInScene();
        HealthEvents.Add(new PlayerHealthEvent { MaxHealth = player.MaxLife, HealthBefore = player.Life, HealthChange = amount, ChangeSource = source, Time = Time.unscaledTime });
    }

    public void AddCoins(int count)
    {
        Coins += count;
        CoinsCollected += count;
    }

    public static void EndRun()
    {
        Instance.RunEndTime = Time.unscaledTime;
        Instance.RunEnded = true;
    }

    public static void Clear()
    {
        Instance = new CurrentRunData();
    }

    public static void StartNewRun()
    {
        Clear();
        Instance.RunStartTime = Time.unscaledTime;
    }
}
