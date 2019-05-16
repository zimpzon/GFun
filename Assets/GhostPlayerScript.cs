using GFun;
using MEC;
using System.Collections.Generic;
using UnityEngine;

public class GhostPlayerScript : MonoBehaviour
{
    public SpriteAnimationFrames_IdleRun Anim;

    [System.NonSerialized]public bool IsStarted;

    TrackedPath path_;
    Transform transform_;
    SpriteRenderer renderer_;

    private void Awake()
    {
        transform_ = transform;
        renderer_ = GetComponent<SpriteRenderer>();
    }

    public void Wander(TrackedPath path, float startDelay)
    {
        path_ = path;
        IsStarted = true;
        Timing.RunCoroutine(WanderCo(startDelay).CancelWith(this.gameObject));
    }

    IEnumerator<float> WanderCo(float startDelay)
    {
        if (!path_.HasPath)
            yield break;

        yield return Timing.WaitForSeconds(startDelay);

        float t = 0;
        float endT = path_.EndT;
        while (true)
        {
            var oldPos = transform_.position;
            var newPos = path_.GetPosAtTime(t);
            transform_.position = newPos;

            var movement = newPos - oldPos;
            bool flipX = movement.x < 0;
            bool isRunning = movement.sqrMagnitude > 0.01f * 0.01f;
            renderer_.sprite = SimpleSpriteAnimator.GetAnimationSprite(isRunning ? Anim.Run : Anim.Idle, Anim.DefaultAnimationFramesPerSecond);
            renderer_.flipX = flipX;

            if (t == 0)
            {
                ParticleScript.EmitAtPosition(ParticleScript.Instance.PlayerLandParticles, newPos, 5);
                ParticleScript.EmitAtPosition(ParticleScript.Instance.WallDestructionParticles, newPos, 5);
                ParticleScript.EmitAtPosition(ParticleScript.Instance.MuzzleFlashParticles, newPos, 1);
            }

            t += Time.unscaledDeltaTime;
            if (t >= endT)
            {
                ParticleScript.EmitAtPosition(ParticleScript.Instance.PlayerLandParticles, newPos, 5);
                ParticleScript.EmitAtPosition(ParticleScript.Instance.WallDestructionParticles, newPos, 5);
                ParticleScript.EmitAtPosition(ParticleScript.Instance.MuzzleFlashParticles, newPos, 1);

                gameObject.SetActive(false);
                yield break;
            }

            yield return 0;
        }
    }
}
