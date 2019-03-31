//using UnityEngine;

//namespace HordeEngine
//{
//    [RequireComponent(typeof(ActorPhysicsBody))]
//    public class ActorRandomMovement : MonoBehaviour, IComponentUpdate
//    {
//        public float MoveSpeed = 3.0f;
//        public SpriteAnimationFrames_IdleRun Anim;
//        public HordeSprite ActorSpriteRenderer;

//        float nextDirChange_;
//        Vector2 dir_;
//        Transform trans_;
//        ActorPhysicsBody actorBody_;

//        private void Awake()
//        {
//            trans_ = transform;
//            actorBody_ = GetComponent<ActorPhysicsBody>();
//        }

//        void OnValidate()
//        {
//            if (Anim != null && Anim.Idle != null && Anim.Idle.Length > 0 && ActorSpriteRenderer != null)
//                ActorSpriteRenderer.Sprite = Anim.Idle[0];
//        }

//        void OnEnable() { Horde.ComponentUpdater.RegisterForUpdate(this, ComponentUpdatePass.Default); }
//        void OnDisable() { Horde.ComponentUpdater.UnregisterForUpdate(this, ComponentUpdatePass.Default); }

//        public void ComponentUpdate(ComponentUpdatePass pass)
//        {
//            float dt = Horde.Time.DeltaSlowableTime;

//            if (Horde.Time.SlowableTime > nextDirChange_)
//            {
//                dir_ = Random.insideUnitCircle;
//                nextDirChange_ = Horde.Time.SlowableTime + Random.value * 3 + 2;
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
