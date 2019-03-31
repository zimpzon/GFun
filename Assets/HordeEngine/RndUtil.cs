using UnityEngine;

static class RndUtil
{
    public static Vector2 RandomInsideUnitCircle()
    {
        return Random.insideUnitCircle;
    }

    public static Vector3 RandomSpread(Vector3 direction, float spread = 15)
    {
        Vector3 dir = direction * spread;
        Vector2 point = RandomInsideUnitCircle();
        dir.x += point.x;
        dir.y += point.y;
        dir.Normalize();
        return dir;
    }
}
