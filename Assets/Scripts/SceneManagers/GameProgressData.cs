﻿using GFun;
using PlayFab.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameProgressData
{
    public static GameProgressData CurrentProgress;
    public static bool EnableSave = false; // So we can start at different scenes without overwriting

    public long NumberOfDeaths;
    public long EnemiesKilled;
    public List<string> UnlockedCharacters = new List<string>() { "Character1" };

    public static void RestartProgress()
    {
        PlayerPrefs.DeleteKey(PlayerPrefsNames.Progress);
    }

    public static bool CharacterIsUnlocked(string tag)
        => CurrentProgress.UnlockedCharacters.Contains(tag);

    public static void LoadProgress()
    {
        string json = PlayerPrefs.GetString(PlayerPrefsNames.Progress);
        if (string.IsNullOrWhiteSpace(json))
        {
            CurrentProgress = new GameProgressData();
            return;
        }

        CurrentProgress = JsonWrapper.DeserializeObject<GameProgressData>(json);
    }

    public static void SaveProgress(bool saveBackup = true)
    {
        if (!EnableSave)
            return;

        // TODO: Might want to obfuscate this
        string json = JsonWrapper.SerializeObject(CurrentProgress);
        PlayerPrefs.SetString(PlayerPrefsNames.Progress, json);
        if (saveBackup)
            PlayerPrefs.SetString(PlayerPrefsNames.Progress + "_backup", json);

        PlayerPrefs.Save();
    }
}