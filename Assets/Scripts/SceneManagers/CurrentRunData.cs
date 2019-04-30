using System;
using System.Collections.Generic;
using UnityEngine;

public enum MapType { Floor, Shop };

[Serializable]
public class CurrentRunData
{
    public static CurrentRunData Instance = new CurrentRunData();
    public bool IsInitialized;

    public MapType NextMapType = MapType.Floor;
    public string StartingCharacterTag = "Character1";
    public int StartingDifficulty = 1;
    public int CurrentFloor = 0;
    public int EnemiesKilled;
    public bool ReaperIsAgitated; // Play AckAck sound and spawn something hard that drops a lot of gold
    public int Coins;
    public int CoinsCollected;
    public int Life;
    public int MaxLife;
    public int PlayerExplosionDamage = 4;
    public float PlayerAttractDistance = 3.0f;

    public float RunStartTime;
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

    public static void Reset()
    {
        Instance = new CurrentRunData();
        Instance.RunStartTime = Time.unscaledTime;
    }
}
