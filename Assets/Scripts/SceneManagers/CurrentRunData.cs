using System;
using System.Collections.Generic;
using UnityEngine;

public enum MapType { Floor, Shop };

[Serializable]
public class CurrentRunData
{
    public static CurrentRunData Instance = new CurrentRunData();
    public bool HasPlayerData;

    public MapType NextMapType = MapType.Floor;
    public string StartingCharacterTag = "Character1";
    public int StartingDifficulty = 1;
    public int CurrentFloor = 0;
    public int Coins;
    public int Life;
    public int MaxLife;
    public int PlayerExplosionDamage = 4;
    public float PlayerAttractDistance = 3.0f;

    public int ShopKeeperPokeCount;
    public int ItemsBought;
    public int CoinsCollected;
    public int EnemiesKilled;

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
