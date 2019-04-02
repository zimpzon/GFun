using UnityEngine;

public class AiBlackboard : MonoBehaviour
{
    public Vector3 PlayerPosition;

    Transform playerTransform_;

    int ThrottleCounter = 0;
    const int ThrottleBucketCount = 30; // Turn will be given every 30th update

    public int GetThrottleHandle()
    {
        return ThrottleCounter++;
    }

    public bool IsItMyTurn(int throttleHandle)
    {
        return (throttleHandle % Time.frameCount == 0);
    }

    private void Start()
    {
        playerTransform_ = SceneGlobals.Instance.PlayerScript.transform;
    }

    private void FixedUpdate()
    {
        PlayerPosition = playerTransform_.position;
    }
}
