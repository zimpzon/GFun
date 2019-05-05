using UnityEngine;

public class BlackHolePluginScript : MapPluginScript
{
    public override void ApplyToMap(Vector3Int position)
    {
        //if (Random.value < 0.75f || CurrentRunData.Instance.CurrentFloor == 1)
        //{
        //    gameObject.SetActive(false);
        //    return;
        //}

        Vector3 worldPosition = new Vector3(MapBuilder.MapMaxWidth / 2, MapBuilder.MapMaxHeight / 2);
        if (Random.value < 0.5f)
        {
            float val = Random.value;
            if (val < 0.5f)
            {
                var p = MapUtil.GetLeftmostFreeCell();
                p.y += 5;
                p.x -= 4;
                worldPosition = p;
            }
            else
            {
                var p = MapUtil.GetRightmostFreeCell();
                p.y += 5;
                p.x += 4;
                worldPosition = p;
            }
        }

        PlayerInfoScript.Instance.ShowInfo("A Gift");

        transform.position = worldPosition;
        ApplyTilemap(new Vector3Int((int)worldPosition.x, (int)worldPosition.y, 0));
    }
}
