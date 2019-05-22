using UnityEngine;

public class World2RandomPluginScript : MapPluginScript
{
    public override string Name => $"Icy Caverns - {CurrentRunData.Instance.FloorInWorld}";

    public override void ApplyToMap(Vector3Int position)
    {
    }

    public override Vector3 GetPlayerStartPosition()
        => MapUtil.GetRandomEdgePosition(out Vector3 unused);
}
