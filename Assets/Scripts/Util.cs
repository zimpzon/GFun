using System.Collections;
using UnityEngine;

public static class Util
{
    public static IEnumerable WaitNoAlloc(float seconds)
    {
        float endTime = Time.time + seconds;
        while (Time.time < endTime)
            yield return null;
    }
}
