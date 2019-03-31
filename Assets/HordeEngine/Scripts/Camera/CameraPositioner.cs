using UnityEngine;

public class CameraPositioner : MonoBehaviour
{
    public float MoveSpeed = 5.0f;
    public Vector3 Target;
    public float TargetOffsetZ;
    public float MoveSpeedZ = 5;
    public float DistanceFromTarget;
    public float DistanceFromTargetOffsetZ;

    Vector3 currentPos_;
    float currentOffsetZ_;
    Transform trans_;

    private void Awake()
    {
        trans_ = transform;
    }

    public void SetTarget(Vector3 target)
    {
        Target = target;
    }

    public void SetPosition(Vector3 pos)
    {
        currentPos_ = pos;
    }

    private void Update()
    {
        UpdateCamera(Time.unscaledDeltaTime);
    }

    public void UpdateCamera(float dt)
    {
        // XY
        var movement = (Target - currentPos_);
        DistanceFromTarget = movement.magnitude;
        movement*= MoveSpeed;

        const float CloseEnough = 0.1f;
        if (DistanceFromTarget > CloseEnough)
            currentPos_ += movement * dt;

        // Z
        float moveZ = (TargetOffsetZ - currentOffsetZ_);
        DistanceFromTargetOffsetZ = moveZ;
        moveZ *= MoveSpeedZ;

        const float CloseEnoughZ = 0.1f;
        if (Mathf.Abs(moveZ) > CloseEnoughZ)
            currentOffsetZ_ += moveZ * dt;

        currentPos_.z = currentOffsetZ_;
        trans_.localPosition = currentPos_;
    }
}
