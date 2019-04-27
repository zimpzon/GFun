using TMPro;
using UnityEngine;

public class FloatingTextScript : MonoBehaviour
{
    Transform transform_;
    TextMeshPro text_;
    GameObjectPool textPool_;
    Vector3 basePos_;
    Vector3 position_;
    float dieTime_;

    private void Awake()
    {
        text_ = GetComponent<TextMeshPro>();
        textPool_ = SceneGlobals.Instance.FloatingTextPool;
        transform_ = transform;
    }

    public void Init(Vector3 position, string text, float timeToLive = 2.0f)
    {
        var go = textPool_.GetFromPool();
        text_.text = text;
        transform_.position = position;
        basePos_ = position;
        position_ = position;
        dieTime_ = Time.unscaledTime + timeToLive;
    }

    public void Die()
    {
        textPool_.ReturnToPool(this.gameObject);
    }

    void Update()
    {
        position_.y += Time.unscaledDeltaTime * 2;
        transform_.position = position_;

        if (Time.unscaledTime >= dieTime_)
            Die();
    }
}
