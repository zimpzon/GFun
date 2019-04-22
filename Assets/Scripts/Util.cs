using UnityEngine;

public static class Util
{
    public static void DebugDrawRect(float x, float y, float w, float h, Color color, float time = 0)
    {
        var bl = new Vector3(x, y);
        var tl = new Vector3(x, y + h);
        var tr = new Vector3(x + w, y + h);
        var br = new Vector3(x + w, y);
        Debug.DrawLine(bl, tl, color, time);
        Debug.DrawLine(tl, tr, color, time);
        Debug.DrawLine(tr, br, color, time);
        Debug.DrawLine(br, bl, color, time);
    }
}
