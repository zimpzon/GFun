using UnityEngine;

// Object to shake will be forced at 0, 0, 0
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    public bool ShakeRotation = true;
    public bool ShakePosition = true;
    public float Dampening = 4.0f;
    public float Scale = 0.2f;
    public float CurrentAmount;
    Transform trans_;

    private void Awake()
    {
        Instance = this;
        trans_ = transform;
    }

    public void SetMinimumShake(float amount)
    {
        if (amount > CurrentAmount)
            CurrentAmount = Mathf.Clamp01(amount);
    }

    void Update()
    {
        float t = Time.unscaledTime * 10.0f;
        float power = CurrentAmount * CurrentAmount * CurrentAmount;
        if (ShakePosition)
        {
            trans_.localPosition = new Vector3(
                Scale * power * (Mathf.PerlinNoise(t + 1, t + 3.33f) - 0.5f),
                Scale * power * (Mathf.PerlinNoise(t + 2, t + 4.44f) - 0.5f) * 0.25f,
                0.0f
            );
        }
        else
        {
            trans_.localPosition = Vector3.zero;
        }

        const float MaxDegrees = 5;
        if (ShakeRotation)
            trans_.localRotation = Quaternion.Euler(0, 0, Scale * power * (Mathf.PerlinNoise(t + 5, t + 6.66f) - 0.5f) * MaxDegrees);
        else
            trans_.localRotation = Quaternion.Euler(0, 0, 0);

        float dt = Time.unscaledDeltaTime;
        CurrentAmount = Mathf.Clamp01(CurrentAmount - dt * Dampening);
	}
}
