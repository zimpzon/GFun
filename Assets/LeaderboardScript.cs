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

    string GetDisplayName(PlayerLeaderboardEntry boardEntry)
        => string.IsNullOrWhiteSpace(boardEntry.DisplayName) ? "(no name)" : boardEntry.DisplayName;

    IEnumerator LoadLeaderboard(string name, int start, int max, Func<int, string> formatScore)
    {
        TextLoading.SetActive(true);

        var playerBoard = StartCoroutine(PlayFabFacade.Instance.GetLeaderboardAroundPlayer(name, 1));
        yield return playerBoard;
        var playerResult = PlayFabFacade.Instance.LastResult as GetLeaderboardAroundPlayerResult;
        if (playerResult != null)
        {
            var boardEntry = playerResult.Leaderboard[0];
            TextPlayerRank.text = (boardEntry.Position + 1).ToString();
            TextPlayerName.text = GetDisplayName(boardEntry);
            TextPlayerScore.text = formatScore(boardEntry.StatValue);
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
            var uiScript = Instantiate(EntryPrefab).GetComponent<LeaderboardUIScript>();
            var boardEntry = result.Leaderboard[i];
            uiScript.SetText(
                (boardEntry.Position + 1).ToString(),
                GetDisplayName(boardEntry),
                formatScore(boardEntry.StatValue));
            uiScript.transform.SetParent(ContentRoot.transform);
            uiScript.transform.localScale = Vector3.one;
            uiScript.transform.localPosition = Vector3.zero;
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
