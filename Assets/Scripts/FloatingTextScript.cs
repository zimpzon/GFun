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
    float speed_;

    private void Awake()
    {
        text_ = GetComponent<TextMeshPro>();
        transform_ = transform;
    }

    public void Init(GameObjectPool textPool, Vector3 position, string text, Color color, float speed = 1.0f, float timeToLive = 2.0f, FontStyles fontStyle = FontStyles.Bold)
    {
        textPool_ = textPool;
        var go = textPool_.GetFromPool();
        text_.text = text;
        text_.color = color;
        text_.fontStyle = fontStyle;
        transform_.position = position;
        basePos_ = position;
        position_ = position;
        speed_ = speed;
        dieTime_ = Time.unscaledTime + timeToLive;
    }

    public void Die()
    {
        textPool_.ReturnToPool(this.gameObject);
    }

    void Update()
    {
        position_.y += Time.unscaledDeltaTime * speed_;
        transform_.position = position_;

        if (Time.unscaledTime >= dieTime_)
            Die();
    }
}
