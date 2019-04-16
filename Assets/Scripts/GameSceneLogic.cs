using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneLogic : MonoBehaviour
{
    public MapStyle MapStyle;
    public Canvas DeadCanvas;
    public string CampSceneName = "TheCampScene";
    public string GameSceneName = "GameScene";
    public Transform DynamicObjectRoot;

    MapScript mapScript_;
    PlayableCharacterScript playerScript_;

    void CreatePlayer()
    {
        string characterTag = CurrentRunData.Instance.StartingCharacterTag;
        var player = PlayableCharacters.Instance.InstantiateCharacter(characterTag, new Vector3(100, 50, 0));
        player.transform.SetParent(DynamicObjectRoot);

        playerScript_ = player.GetComponent<PlayableCharacterScript>();
        playerScript_.Life = CurrentRunData.Instance.Life;
    }

    private void Awake()
    {
        mapScript_ = SceneGlobals.Instance.MapScript;
    }

    private void Start()
    {
        CreatePlayer();
        GenerateMap();

        PlayableCharacters.Instance.SetCharacterToHumanControlled(playerScript_.tag);
        Helpers.SetCameraPositionToActivePlayer();

        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                playerScript_.Die();
            }

            if (playerScript_.IsDead)
            {
                StartCoroutine(DeadLoop());
                yield break;
            }

            yield return null;
        }
    }

    IEnumerator DeadLoop()
    {
        DeadCanvas.gameObject.SetActive(true);

        GameProgressData.CurrentProgress.NumberOfDeaths++;
        GameProgressData.SaveProgress();

        while (true)
        {
            if (Input.GetKeyDown(KeyCode.R))
                SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);

            if (Input.GetKeyDown(KeyCode.Space))
                SceneManager.LoadScene(CampSceneName, LoadSceneMode.Single);

            yield return null;
        }
    }

    public void GenerateMap()
    {
        int w = 100;
        int h = 50;

        MapBuilder.GenerateMapFloor(w, h, MapFloorAlgorithm.RandomWalkers);
        MapBuilder.Fillrect(new Vector2Int(90, 55), 15, 3, 1);
        MapBuilder.BuildMapTiles(MapBuilder.MapSource, mapScript_, MapStyle);
    }
}
