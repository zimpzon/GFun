using GFun;
using System.Collections;
using UnityEngine;

public class EnemyScript : MonoBehaviour, IMovableActor, ISensingActor
{
    public float Speed = 1;
    public float Life = 50;
    public float Drag = 1.0f;
    public bool IsDead = false;
    public SpriteRenderer BlipRenderer;
    public SpriteRenderer ShadowRenderer;
    public SpriteRenderer LightRenderer;

    bool checkPlayerLos_;
    float playerLosTime_;
    Vector3 playerLatestKnownPosition_;

    Transform transform_;
    Rigidbody2D body_;
    IMapAccess map_;
    ISpriteAnimator spriteAnimator_;
    Vector3 force_;
    Vector3 moveRequest_;
    Vector3 latestFixedMovenentDirection_;
    float flashAmount_;
    float flashEndTime_;
    Material renderMaterial_;
    SpriteRenderer renderer_;
    Collider2D collider_;

    void Awake()
    {
        transform_ = transform;
        body_ = GetComponent<Rigidbody2D>();
        map_ = SceneGlobals.Instance.MapAccess;
        spriteAnimator_ = GetComponent<ISpriteAnimator>();
        renderer_ = GetComponent<SpriteRenderer>();
        renderMaterial_ = renderer_.material;
        collider_ = GetComponent<Collider2D>();
        BlipRenderer.enabled = true;
    }

    // ISensingActor
    public void SetCheckPlayerLoS(bool doCheck)
        => checkPlayerLos_ = doCheck;

    public Vector3 GetPlayerLatestKnownPosition()
        => playerLatestKnownPosition_;

    public float GetPlayerLoSAge()
        => Time.time - playerLosTime_;

    // IMovableActor
    public Vector3 GetPosition()
        => transform_.position;

    public void SetMovementVector(Vector3 vector, bool isNormalized = true)
    {
        moveRequest_ = isNormalized ? vector : vector.normalized;
    }

    public void SetMinimumForce(Vector3 force)
    {
        if (force.sqrMagnitude > force_.sqrMagnitude)
            force_ = force;
    }

    public void TakeDamage(int amount, Vector3 damageForce)
    {
        DoFlash(2, 0.1f);
        SetForce(damageForce);
        Life = Mathf.Min(0, Life - amount);
        if (Life == 0)
            Die();
    }

    public void Die()
    {
        DoFlash(-0.5f, 1.0f);
        IsDead = true;
        this.gameObject.layer = SceneGlobals.Instance.DeadEnemyLayer;
        body_.bodyType = RigidbodyType2D.Dynamic;
        body_.AddForceAtPosition(force_ * 2000, Vector2.zero);
        body_.freezeRotation = false;
        BlipRenderer.enabled = false;
        LightRenderer.enabled = false;
        ShadowRenderer.enabled = false;

        StartCoroutine(DieCo());
    }

    IEnumerator DieCo()
    {
        SceneGlobals.Instance.CameraShake.SetMinimumShake(0.2f);
        yield return new WaitForSeconds(1.5f);
        //var particles = SceneGlobals.Instance.ParticleScript.WallDestructionParticles;
        //ParticleScript.EmitAtPosition(particles, transform_.position, 20);
        this.gameObject.SetActive(false);
    }

    public void SetForce(Vector3 force)
    {
        force_ = force;
    }

    void FixedUpdateInternal(float dt)
    {
        Vector3 movement = moveRequest_ * Speed * dt + force_ * dt;
        latestFixedMovenentDirection_ = movement.normalized;
        moveRequest_ = Vector3.zero;

        if (force_.sqrMagnitude > 0.0f)
        {
            float forceLen = force_.magnitude;
            forceLen = Mathf.Clamp(forceLen - Drag * dt, 0.0f, float.MaxValue);
            force_ = force_.normalized * forceLen;
        }

        bool isRunning = movement != Vector3.zero;
        if (isRunning)
            body_.MovePosition(transform_.position + movement);
    }

    static int LosThrottleCounter = 0;
    int myLosThrottleId_ = LosThrottleCounter++;

    void DoFlash(float amount, float ms)
    {
        flashEndTime_ = Time.unscaledTime + ms;
        flashAmount_ = amount;
        renderMaterial_.SetFloat("_FlashAmount", flashAmount_);
    }

    void UpdateFlash()
    {
        if (flashAmount_ > 0.0f && Time.unscaledTime > flashEndTime_)
        {
            flashAmount_ = 0.0f;
            renderMaterial_.SetFloat("_FlashAmount", 0.0f);
        }
    }

    void Update()
    {
        UpdateFlash();
        if (!IsDead)
        {
            UpdateLos();
            spriteAnimator_.UpdateAnimation(latestFixedMovenentDirection_);
        }
    }

    private void FixedUpdate()
    {
        if (!IsDead)
        {
            FixedUpdateInternal(Time.fixedDeltaTime);
        }
    }

    private void UpdateLos()
    {
        bool checkNow = (Time.frameCount + myLosThrottleId_) % AiBlackboard.LosThrottleModulus == 0;
    }
}
