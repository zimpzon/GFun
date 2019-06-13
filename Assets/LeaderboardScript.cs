using PlayFab.ClientModels;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class LeaderboardScript : MonoBehaviour
{
    public static LeaderboardScript Instance;

    public GameObject ContentRoot;
    public GameObject EntryPrefab;
    public AudioClip ShowSound;
    public AudioClip CloseSound;

    public TextMeshProUGUI TextPlayerRank;
    public TextMeshProUGUI TextPlayerName;
    public TextMeshProUGUI TextPlayerScore;
    public GameObject TextLoading;

    public void Close()
    {
        this.gameObject.SetActive(false);
        AudioManager.Instance.PlaySfxClip(CloseSound, 1);
    }

    public void Show()
    {
        this.gameObject.SetActive(true);

        while (ContentRoot.transform.childCount > 0)
            DestroyImmediate(ContentRoot.transform.GetChild(0).gameObject);

        StartCoroutine(LoadLeaderboard(PlayFabData.Stat_FloorReached, start: 0, max: 20, (int score) => $"{score}"));

        AudioManager.Instance.PlaySfxClip(ShowSound, 1);
    }

    IEnumerator LoadLeaderboard(string name, int start, int max, Func<int, string> formatScore)
    {
        TextLoading.SetActive(true);

        var playerBoard = StartCoroutine(PlayFabFacade.Instance.GetLeaderboardAroundPlayer(name, max));
        yield return playerBoard;
        var playerResult = PlayFabFacade.Instance.LastResult as GetLeaderboardAroundPlayerResult;
        if (playerResult != null)
        {
            TextPlayerRank.text = (playerResult.Leaderboard[0].Position + 1).ToString();
            TextPlayerName.text = playerResult.Leaderboard[0].DisplayName;
            TextPlayerScore.text = formatScore(playerResult.Leaderboard[0].StatValue);
        }
        else
        {
            TextPlayerRank.text = "(not found)";
            TextPlayerName.text = "(not found)";
            TextPlayerScore.text = "(not found)";
        }

        var globalBoard = StartCoroutine(PlayFabFacade.Instance.GetLeaderboard(name, start, max));
        yield return globalBoard;

        var result = PlayFabFacade.Instance.LastResult as GetLeaderboardResult;
        for (int i = 0; i < result.Leaderboard.Count; ++i)
        {
            var entryScript = Instantiate(EntryPrefab).GetComponent<LeaderboardUIScript>();
            var board = result.Leaderboard[i];
            entryScript.SetText(
                (board.Position + 1).ToString(),
                string.IsNullOrWhiteSpace(board.DisplayName) ? "(no name)" : board.DisplayName,
                formatScore(board.StatValue));
            entryScript.transform.SetParent(ContentRoot.transform);
            entryScript.transform.localScale = Vector3.one;
            entryScript.transform.localPosition = Vector3.zero;
        }

        TextLoading.SetActive(false);
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            Close();
    }
}
