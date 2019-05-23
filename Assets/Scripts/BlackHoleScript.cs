using UnityEngine;

public class BlackHoleScript : MonoBehaviour
{
    public Transform SpriteTransform;

    float baseScale_;

    private void Awake()
    {
        baseScale_ = SpriteTransform.localScale.x; // Assuming uniform scale
    }

    public void OnPlayerEnterered()
    {
        FloatingTextSpawner.Instance.Spawn(SpriteTransform.position + Vector3.up * 3, "Not Implemented", Color.yellow, 0.1f, 3.0f);
    }

    void Update()
    {
        float sinX = Mathf.Sin(Time.time * 10.1f) * 0.05f;
        float sinY = Mathf.Sin(Time.time * 9.01f) * 0.05f;
        SpriteTransform.localScale = new Vector3(baseScale_ + sinX, baseScale_ + sinY, 1);
    }
}
