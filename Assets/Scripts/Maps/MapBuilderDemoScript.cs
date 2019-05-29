using UnityEngine;
using UnityEngine.UI;

public class MapBuilderDemoScript : MonoBehaviour
{
    public Dropdown DropdownAlgo;
    public Button BtnGenerateNewFloor;
    public Toggle ToggleBackground;
    public MapStyle MapStyle;

    MapScript mapScript_;

    private void Start()
    {
        mapScript_ = SceneGlobals.Instance.MapScript;

        BtnGenerateNewFloor.onClick.AddListener(GenerateNewMap);
        ToggleBackground.onValueChanged.AddListener(DoToggleBackground);

        GenerateNewMap();

        CreatePlayer();
    }

    void CreatePlayer()
    {
        PlayableCharacters.Instance.InstantiateCharacter("Character1", new Vector3(100, 50, 0));
        PlayableCharacters.GetPlayerInScene().SetIsHumanControlled(true, constraints: RigidbodyConstraints2D.FreezeRotation);
        Helpers.SetCameraPositionToActivePlayer();
    }

    private void Update()
    {
        float z = Camera.main.transform.localPosition.z;
        if (Input.GetKeyDown(KeyCode.Z))
            Camera.main.transform.localPosition = new Vector3(0, 0, z - 5);
        if (Input.GetKeyDown(KeyCode.X))
            Camera.main.transform.localPosition = new Vector3(0, 0, z + 5);

        if (Input.GetKeyDown(KeyCode.C))
        {
            var col = SceneGlobals.Instance.LightingCamera.GetAmbientLightColor();
            Color.RGBToHSV(col, out float h, out float s, out float v);
            v = Mathf.Clamp01(v + 0.1f);
            SceneGlobals.Instance.LightingCamera.SetAmbientLightColor(Color.HSVToRGB(h, s, v));
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            var col = SceneGlobals.Instance.LightingCamera.GetAmbientLightColor();
            Color.RGBToHSV(col, out float h, out float s, out float v);
            v = Mathf.Clamp01(v - 0.1f);
            SceneGlobals.Instance.LightingCamera.SetAmbientLightColor(Color.HSVToRGB(h, s, v));
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            float clarity = mapScript_.GetWallClarity();
            mapScript_.SetWallClarity(clarity + 0.1f);
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            float clarity = mapScript_.GetWallClarity();
            mapScript_.SetWallClarity(clarity - 0.1f);
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            mapScript_.BuildCollisionMapFromFloorTilemap(mapScript_.FloorTileMap);
        }
    }

    public void DoToggleBackground(bool active)
    {
        mapScript_.BackgroundQuad.SetActive(active);
    }

    public void GenerateNewMap()
    {
        string strAlgo = DropdownAlgo.options[DropdownAlgo.value].text;
        var algo = (MapFloorAlgorithm)System.Enum.Parse(typeof(MapFloorAlgorithm), strAlgo);
        int w = 50;
        int h = 50;

        switch(algo)
        {
            case MapFloorAlgorithm.SingleRoom:
                w = Random.Range(5, 15);
                h = Random.Range(4, 12);
                break;
            case MapFloorAlgorithm.RandomWalkers:
                break;
            case MapFloorAlgorithm.CaveLike1:
                break;
        }

        MapBuilder.GenerateMapFloor(w, h, algo);
        MapBuilder.BuildMap(MapBuilder.MapSource, mapScript_, MapStyle);
    }
}
