using MEC;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneLogic : MonoBehaviour
{
    public MapStyle MapStyle;
    public Canvas DeadCanvas;
    public Canvas InitCanvas;
    public string CampSceneName = "TheCampScene";
    public string GameSceneName = "GameScene";
    public Transform DynamicObjectRoot;
    public AudioClip PlayerLandSound;
   
    MapScript map_;
    PlayableCharacterScript playerScript_;

    void CreatePlayer(Vector3 position)
    {
        string characterTag = CurrentRunData.Instance.StartingCharacterTag;
        var player = PlayableCharacters.Instance.InstantiateCharacter(characterTag, position);
        player.transform.SetParent(DynamicObjectRoot);

        playerScript_ = player.GetComponent<PlayableCharacterScript>();
        playerScript_.Life = CurrentRunData.Instance.Life;
    }

    private void Awake()
    {
        map_ = SceneGlobals.Instance.MapScript;
    }

    private void Start()
    {
        Timing.RunCoroutine(EnterLevelLoop());
    }

    IEnumerator<float> EnterLevelLoop()
    {
        InitCanvas.enabled = true;
        yield return 0;

        float startTime = Time.time;
        GenerateMap();

        CreatePlayer(map_.GetPlayerStartPosition());

        PlayableCharacters.Instance.SetCharacterToHumanControlled(playerScript_.tag);
        Helpers.SetCameraPositionToActivePlayer();

        const float MinimumShowTime = 1.5f;
        float timeLeft = (startTime - Time.time) + MinimumShowTime;
        yield return Timing.WaitForSeconds(timeLeft);

        InitCanvas.enabled = false;

        var playerInScene = PlayableCharacters.GetPlayerInScene();
        var targetPos = playerInScene.transform.position;
        var startOffset = new Vector3(0, 1, -12);
        float t = 1;
        SceneGlobals.Instance.AudioManager.PlaySfxClip(SceneGlobals.Instance.AudioManager.AudioClips.PlayerLand, 1, 0.1f);
        while (t >= 0)
        {
            CameraShake.Instance.SetMinimumShake(0.5f);

            var pos = targetPos + startOffset * t;
            playerInScene.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, 0, t * 500));

            t -= Time.unscaledDeltaTime * 2;
            yield return 0;
        }

        playerInScene.transform.SetPositionAndRotation(targetPos, Quaternion.identity);
        map_.ExplodeWalls(targetPos, 3);

        var playerCenter = targetPos + Vector3.up * 0.5f;
        ParticleScript.EmitAtPosition(ParticleScript.Instance.PlayerLandParticles, playerCenter, 15);
        ParticleScript.EmitAtPosition(ParticleScript.Instance.MuzzleFlashParticles, playerCenter, 1);
        ParticleScript.EmitAtPosition(ParticleScript.Instance.WallDestructionParticles, playerCenter, 5);

        float shakeEndTime = Time.time + 0.3f;
        while (Time.time < shakeEndTime)
        {
            CameraShake.Instance.SetMinimumShake(1.0f);
            yield return 0;
        }

        Timing.RunCoroutine(GameLoop().CancelWith(gameObject));
        yield return 0;
    }

    IEnumerator<float> GameLoop()
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

            yield return 0;
        }
    }

    IEnumerator<float> DeadLoop()
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

            yield return 0;
        }
    }

    public void GenerateMap()
    {
        int w = 100;
        int h = 50;

        MapBuilder.GenerateMapFloor(w, h, MapFloorAlgorithm.RandomWalkers);
        MapBuilder.Fillrect(new Vector2Int(90, 55), 15, 3, 1);
        MapBuilder.BuildMapTiles(MapBuilder.MapSource, map_, MapStyle);
    }
}
