using System;
using System.Collections.Generic;

[Serializable]
public class CurrentRunData
{
    public static CurrentRunData Instance = new CurrentRunData();

    public bool IsInitialized;

    public string StartingCharacterTag = "Character1";
    public int StartingDifficulty = 1;
    public int CurrentDifficulty = 1;
    public int EnemiesKilled;

    public int Coins;
    public int Life;
    public int MaxLife;
    public float PlayerAttractDistance = 3.0f;

    public List<string> NewUnlockedCharacters = new List<string>();

    public static void Reset()
    {
        Instance = new CurrentRunData();
    }
}
