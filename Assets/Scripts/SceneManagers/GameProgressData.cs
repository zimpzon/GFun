using GFun;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameProgressData
{
    public static GameProgressData CurrentProgress = new GameProgressData();
    public static bool EnableSave = false; // So we can start at different scenes without overwriting

    public int Version = 3;
    public const int RequiredVersion = 3;
    public int NumberOfDeaths;
    public float DamageBonusPerLevel = 0.02f;
    public QuestProgress QuestProgress = new QuestProgress();

    public List<string> UnlockedCharacters = new List<string>() { "Character1", "Character2", "Character3" };

    public static void RestartProgress()
    {
        PlayerPrefs.DeleteKey(PlayerPrefsNames.Progress);
        LoadProgress();
    }

    public static bool CharacterIsUnlocked(string tag)
        => CurrentProgress.UnlockedCharacters.Contains(tag);

    public static void LoadProgress()
    {
        string json = PlayerPrefs.GetString(PlayerPrefsNames.Progress);
        if (string.IsNullOrWhiteSpace(json))
            CurrentProgress = new GameProgressData();
        else
            CurrentProgress = JsonUtility.FromJson<GameProgressData>(json);

        if (CurrentProgress.Version < GameProgressData.RequiredVersion)
            CurrentProgress = new GameProgressData();
    }

    public static void SaveProgress(bool saveBackup = true)
    {
        if (!EnableSave)
            return;

        // TODO: Might want to obfuscate this
        string json = JsonUtility.ToJson(CurrentProgress);
        PlayerPrefs.SetString(PlayerPrefsNames.Progress, json);
        if (saveBackup)
            PlayerPrefs.SetString(PlayerPrefsNames.Progress + "_backup", json);

        PlayerPrefs.Save();
    }
}
