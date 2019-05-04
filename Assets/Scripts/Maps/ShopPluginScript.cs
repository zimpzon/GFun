using UnityEngine;

public class ShopPluginScript : MapPluginScript
{
    public Transform PortalPosition;

    public override void ApplyToMap(Vector3Int position)
    {
        ApplyTilemap(position);
    }
}
