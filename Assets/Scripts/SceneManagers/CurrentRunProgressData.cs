using System;
using System.Collections.Generic;

[Serializable]
public class CurrentRunData
{
    public static CurrentRunData Instance;

    // Starting parameters


    // Dynamic parameters

    public int Life;
    public long EnemiesKilled;
    public List<string> UnlockedCharacters = new List<string>();

    public static void Reset()
    {
        //Instance = new GameProgressData();
    }
}
