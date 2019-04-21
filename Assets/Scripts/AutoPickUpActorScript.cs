using UnityEngine;

public enum AutoPickUpType { None, Coin, Ammo, Health, }

public class AutoPickUpActorScript : MonoBehaviour
{
    public AutoPickUpType Type;
    public int Value = 1;
    public float TimeToLive = 10;
    public float PickupDistance = 0.8f;
    float AttractPower = 20.0f;
    float FlashAmount = 100.0f;
    float FlashSeconds = 3;
    public AudioClip PickUpSound;
    public GameObjectPool ObjectPool;

    SpriteRenderer renderer_;
    Material renderMaterial_;
    float throwEndTime_;
    float expireTime_;
    Transform transform_;
    PlayableCharacterScript player_;
    MapScript map_;
    float sqrPickupDistance_;
    Vector3 force_;

    private void Awake()
    {
        transform_ = transform;
        sqrPickupDistance_ = PickupDistance * PickupDistance;
        map_ = SceneGlobals.Instance.MapScript;
        renderer_ = GetComponent<SpriteRenderer>();
        renderMaterial_ = renderer_.material;
    }

    public void Throw(Vector3 force)
    {
        float time = Time.time;
        force_ = force;
        throwEndTime_ = time + 0.5f;
        expireTime_ = time + TimeToLive + Random.value * 0.3f;
    }

    void Die()
    {
        renderMaterial_.SetFloat("_FlashAmount", 0.0f);

        if (ObjectPool != null)
        {
            ObjectPool.ReturnToPool(gameObject);
            ObjectPool = null;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void UpdateFlash(float time, float expireTime)
    {
        if (expireTime - time < FlashSeconds)
        {
            bool flashOn = ((int)(time * 10) & 1) == 0;
            renderMaterial_.SetFloat("_FlashAmount", flashOn ? FlashAmount : 0.0f);
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        float time = Time.time;
        var myPos = transform_.position;

        UpdateFlash(time, expireTime_);

        var playerPos = AiBlackboard.Instance.PlayerPosition + Vector3.up * 0.5f;
        var diff = playerPos - myPos;
        float sqrDistance = CurrentRunData.Instance.PlayerAttractDistance * CurrentRunData.Instance.PlayerAttractDistance;
        if (diff.sqrMagnitude < sqrDistance && time > throwEndTime_)
        {
            var direction = diff.normalized;
            force_ = direction * AttractPower;
        }

        float forceMagnitude = force_.magnitude;
        if (forceMagnitude > 0.1f)
        {
            var newPos = myPos + force_ * dt;
            const float LookAheadSize = 0.5f;
            var lookAhead = newPos + force_.normalized * LookAheadSize;
            if (map_.GetCollisionTileValue(lookAhead) == MapBuilder.TileWalkable)
            {
                transform_.position = newPos;
            }

            forceMagnitude -= dt * 2;
            force_ = Vector3.ClampMagnitude(force_, forceMagnitude);
        }

        if (diff.sqrMagnitude < sqrPickupDistance_ && time > throwEndTime_)
        {
            AudioManager.Instance.PlaySfxClip(PickUpSound, 1);
            GameEvents.RaiseAutoPickUp(Type, Value, transform_.position);
            Die();
        }

        if (time > expireTime_)
            Die();
    }
}
