using UnityEngine;

public class AiBlackboard : MonoBehaviour
{
    public static AiBlackboard Instance;
    public const int LosThrottleModulus = 15;

    public Vector3 PlayerPosition;
    public bool BulletTimeActive;

    void Awake()
    {
        Instance = this;
    }
}
