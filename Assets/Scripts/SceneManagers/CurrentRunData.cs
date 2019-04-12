using System;
using System.Collections.Generic;

[Serializable]
public class CurrentRunData
{
    public static CurrentRunData Instance = new CurrentRunData();

    public string StartingCharacterTag = "Character1";
    public int StartingDifficulty = 1;
    public int CurrentDifficulty = 1;
    public int Life = 1;
    public long EnemiesKilled;
    public List<string> NewUnlockedCharacters = new List<string>();

    public static void Reset()
    {
        Instance = new CurrentRunData();
    }
}
