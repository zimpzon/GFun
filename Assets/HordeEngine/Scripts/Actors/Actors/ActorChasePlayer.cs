//using System;
//using TMPro;
//using UnityEngine;

//namespace HordeEngine
//{
//    [RequireComponent(typeof(ActorPhysicsBody))]
//    public class ActorChasePlayer : MonoBehaviour, IComponentUpdate
//    {
//        public float MoveSpeed = 3.0f;
//        public SpriteAnimationFrames_IdleRun Anim;
//        public HordeSprite ActorSpriteRenderer;
//        public TextMeshPro Text;
//        [NonSerialized]public bool IsChasing;

//        float timeNextDirChange_;
//        Vector2 dir_;
//        Transform trans_;
//        ActorPhysicsBody actorBody_;
//        ActorLineOfSight actorLoS_;

//        private void Awake()
//        {
//            trans_ = transform;
//            actorBody_ = GetComponent<ActorPhysicsBody>();
//            actorLoS_ = GetComponent<ActorLineOfSight>();
//        }

//        void OnValidate()
//        {
//            if (Anim != null && Anim.Idle != null && Anim.Idle.Length > 0 && ActorSpriteRenderer != null)
//                ActorSpriteRenderer.Sprite = Anim.Idle[0];
//        }

//        void OnEnable() { Horde.ComponentUpdater.RegisterForUpdate(this, ComponentUpdatePass.Default); }
//        void OnDisable() { Horde.ComponentUpdater.UnregisterForUpdate(this, ComponentUpdatePass.Default); }

//        void Fire()
//        {
//            ProjectileSpawners.SpawnSingle(
//                Global.SceneAccess.ProjectileBlueprints.Bullet1,
//                collidePlayer: true,
//                actorBody_.Position,
//                dir_,
//                speed: 5,
//                Global.SceneAccess.ProjectileManager,
//                ProjectileUpdaters.CirclingMove);
//        }

//        public void ComponentUpdate(ComponentUpdatePass pass)
//        {
//            float dt = Horde.Time.DeltaSlowableTime;

//            bool hasRecentlySeenPlayer = actorLoS_.LatestLineOfSightAge < 3;

//            if (hasRecentlySeenPlayer && !IsChasing)
//                // Just noticed player
//                timeNextDirChange_ = 0;

//            if (Horde.Time.SlowableTime > timeNextDirChange_)
//            {
//                if (hasRecentlySeenPlayer)
//                {
//                    dir_ = (actorLoS_.LatestLineOfSightPosition - actorBody_.Position).normalized;
//                    timeNextDirChange_ = Horde.Time.SlowableTime + 0.25f;
//                    IsChasing = true;
//                    Text.text = "!";

//                    if (actorLoS_.HasLineOfSight && UnityEngine.Random.value < 0.5)
//                        Fire();
//                }
//                else
//                {
//                    // Move randomly if players position is unknown
//                    dir_ = UnityEngine.Random.insideUnitCircle;
//                    timeNextDirChange_ = Horde.Time.SlowableTime + 1 + UnityEngine.Random.value;
//                    IsChasing = false;
//                    Text.text = "?";
//                }
//            }

//            bool isRunning = dir_.sqrMagnitude > 0.0f;
//            if (isRunning)
//            {
//                var velocity = dir_.normalized * dt * MoveSpeed;
//                actorBody_.Move(velocity);
//            }

//            ActorSpriteRenderer.Sprite = SimpleSpriteAnimator.GetAnimationSprite(isRunning ? Anim.Run : Anim.Idle, Anim.DefaultAnimationFramesPerSecond);
//            ActorSpriteRenderer.FlipX = dir_.x < 0.0f;
//        }
//    }
//}
