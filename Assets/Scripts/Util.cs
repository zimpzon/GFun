using UnityEngine;

public static class Util
{
    /// <summary>
    /// Returns the closest of Up, Down, Left or Right.
    /// </summary>
    public static Vector3 GetGenerelDirection(Vector3 from, Vector3 to)
    {
        var diff = to - from;

        if (Mathf.Abs(diff.y) > Mathf.Abs(diff.x))
            return diff.y > 0 ? Vector3.up : Vector3.down;
        else
            return diff.x > 0 ? Vector3.right : Vector3.left;
    }

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
