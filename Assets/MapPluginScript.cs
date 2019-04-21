using UnityEngine;

public class MapPluginScript : MonoBehaviour
{
    public string Name;
    public int Width = 20;
    public int Height = 10;

    Vector2Int tilePosition_ = new Vector2Int();

    public void ApplyToMap()
    {
        var pos = transform.position;
        int x = (int)pos.x;
        int y = (int)pos.y;

        print($"Applying plugin {Name} at {x}, {y} -> {x + Width - 1}, {y + Height - 1}");
        MapBuilder.Fillrect(x, y, Width, Height, 1);
    }

    private void OnDrawGizmos()
    {
        var pos = transform.position;
        int x = (int)pos.x - Width / 2;
        int y = (int)pos.y - Height / 2;
        Util.DebugDrawRect(x, y, Width, Height, Color.green, 0);
    }
}
