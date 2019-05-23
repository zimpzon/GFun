using UnityEngine;

public class ShieldScript : MonoBehaviour
{
    Transform transform_;
    Vector3 baseScale_;

    void Start()
    {
        transform_ = transform;
        baseScale_ = transform_.localScale;
    }

    void Update()
    {
        var scale = baseScale_;
        float sinX = Mathf.Sin(Time.time * 18.124f) * 0.25f;
        float sinY = Mathf.Sin(Time.time * 15.56f) * 0.25f;
        scale.x += sinX;
        scale.y += sinY;
        transform_.localScale = scale;
    }
}
