using UnityEngine;

public class AiBlackboard : MonoBehaviour
{
    public static AiBlackboard Instance;
    public const int LosThrottleModulus = 30;

    public Vector3 PlayerPosition;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        
    }
}
