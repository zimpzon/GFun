using System;
using System.Collections.Generic;

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
    public int Life;
    public int MaxLife;
    public int PlayerExplosionDamage = 4;
    public float PlayerAttractDistance = 3.0f;

    public List<string> NewUnlockedCharacters = new List<string>();

    public static void Reset()
    {
        Instance = new CurrentRunData();
    }
}
