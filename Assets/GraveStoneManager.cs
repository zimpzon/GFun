using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GraveStoneManager : MonoBehaviour
{
    public static GraveStoneManager Instance;

    public Tile[] GraveStoneWall;
    public Tile[] GraveStoneTop;
    public Tile GraveyardFloor;

    public GameObject GravestoneInteractPrefab;
    public GameObject PlayerInfoPresenterPrefab;

    private static int GraveStoneXMax = -13;
    private static int GraveStoneXMin = -32;
    private static int GraveStoneYMax = 4;
    private static int GraveStoneYMin = -14;
    private static int GraveStoneStartX = -9;
    private static int GraveStoneStartY = -6;

    private void Awake()
    {
        Instance = this;
        GameProgressData.LoadProgress();
    }

    // Start is called before the first frame update
    public void CreateGravestones()
    {
        int gameProgressIndex = 0;
        // TODO: Load all the past deaths information here when we update instead of just the one
        List<GameProgressData> gameProgress = new List<GameProgressData>();
        gameProgress.Add(GameProgressData.CurrentProgress);
        for (int i = 0; i < gameProgress[gameProgressIndex].NumberOfDeaths; i++)
        {
            gameProgress.Add(GameProgressData.CurrentProgress);
        }
        // going from right to left instead of left to right to fill in closer to the graveyard entrance first
        for (int xPos = GraveStoneStartX; xPos > GraveStoneXMin && gameProgressIndex < gameProgress.Count; xPos -= 2)
        {
            int yPos = xPos > GraveStoneXMax ? GraveStoneStartY : GraveStoneYMax;
            // filling in top to bottom, separate by 2 due to the gravestone top and so the players have a row to move between
            for (; yPos > GraveStoneYMin && gameProgressIndex < gameProgress.Count; yPos -= 2)
            {
                string info = $"Enemies Killed = {gameProgress[gameProgressIndex].EnemiesKilled} Deaths = {gameProgress[gameProgressIndex].NumberOfDeaths} ";
                CreateGraveStone(xPos, yPos, info);
                gameProgressIndex++;
            }
        }
    }

    private void CreateGraveStone(int xpos, int ypos, string info)
    {
        int randomGrave = Random.Range(0, GraveStoneWall.Length);
        MapScript.Instance.WallTileMap.SetTile(new Vector3Int(xpos, ypos, 0), GraveStoneWall[randomGrave]);
        MapScript.Instance.TopTileMap.SetTile(new Vector3Int(xpos, ypos + 1, 0), GraveStoneTop[randomGrave]);
        GridLayout gridLayout = MapScript.Instance.WallTileMap.GetComponentInParent<GridLayout>();
        // add 0.5 in x and y so it's in the center of the cell
        Vector3 worldPosition = gridLayout.CellToWorld(new Vector3Int(xpos, ypos, 0)) + new Vector3(0.5f, 0.5f, 0);
        GameObject grave = Instantiate(GravestoneInteractPrefab, worldPosition, Quaternion.identity);
        grave.GetComponent<InteractableTrigger>().OnAccept.AddListener(delegate { PlayerInfoPresenterPrefab.GetComponent<PlayerInfoScript>().ShowInfo(info); });
    }
}
