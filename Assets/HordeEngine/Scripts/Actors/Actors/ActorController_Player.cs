//using UnityEngine;

//namespace HordeEngine
//{
//    [RequireComponent(typeof(ActorPhysicsBody))]
//    public class ActorController_Player : MonoBehaviour, IComponentUpdate
//    {
//        public float MoveSpeed = 5.0f;
//        public SpriteAnimationFrames_IdleRun Anim;
//        public HordeSprite PlayerSpriteRenderer;

//        Transform trans_;
//        ActorPhysicsBody actorBody_;
//        ActorAbility_Dodge dodge_;
//        bool flipX_;

//        private void Awake()
//        {
//            trans_ = transform;
//            actorBody_ = GetComponent<ActorPhysicsBody>();
//            dodge_ = GetComponent<ActorAbility_Dodge>();

//            PlayerCollision.PlayerSize = actorBody_.CollisionCircleSize;
//            PlayerCollision.OnPlayerCollision = Hit;
//            PlayerCollision.PlayerBody = actorBody_;

//            ReportPosition();
//        }

//        void OnValidate()
//        {
//            if (Anim != null && Anim.Idle != null && Anim.Idle.Length > 0 && PlayerSpriteRenderer != null)
//                PlayerSpriteRenderer.Sprite = Anim.Idle[0];
//        }

//        void OnEnable() { Horde.ComponentUpdater.RegisterForUpdate(this, ComponentUpdatePass.Default); }
//        void OnDisable() { Horde.ComponentUpdater.UnregisterForUpdate(this, ComponentUpdatePass.Default); }

//        Vector2 frameForce_;
//        public void Hit(ref Projectile p, Vector2 velocity)
//        {
//            if (dodge_.IsDodging)
//                return;

//            frameForce_ += velocity.normalized;
//        }

//        public void ReportPosition()
//        {
//            var playerPos = trans_.localPosition + (Vector3)actorBody_.CollisionCircleOffset;
//            PlayerCollision.PlayerPos = playerPos;
//            Horde.AiBlackboard.PlayerPos = playerPos;
//        }

//        bool bulletTime_;
//        float bulletTimeValue_;
//        float bulletTimeTarget_;
//        void ToggleBulletTime()
//        {
//            bulletTime_ = !bulletTime_;
//            bulletTimeTarget_ = bulletTime_ ? 1.0f : 0.0f;
//        }

//        void UpdateBulletTime()
//        {
//            Global.SceneAccess.LightingImageEffect.MonochromeAmount = 2.0f * bulletTimeValue_;
//            Global.SceneAccess.Music.pitch = 1.0f - (0.2f * bulletTimeValue_);
//            Horde.Time.SlowableTimeScale = 1.0f - bulletTimeValue_;

//            float diff = bulletTimeTarget_ - bulletTimeValue_;
//            bulletTimeValue_ += Mathf.Sign(diff) * diff * diff * Horde.Time.Time * 0.01f;
//            bulletTimeValue_ = Mathf.Clamp01(bulletTimeValue_);
//        }

//        void Fire(Vector2 dir)
//        {
//            ProjectileSpawners.SpawnSingle(
//                Global.SceneAccess.ProjectileBlueprints.Bullet1,
//                false,
//                trans_.localPosition,
//                dir,
//                20.0f,
//                Global.SceneAccess.ProjectileManager,
//                ProjectileUpdaters.BasicMove);
//        }

//        public void ComponentUpdate(ComponentUpdatePass pass)
//        {
//            UpdateBulletTime();

//            actorBody_.AddForce(frameForce_.normalized * 2);
//            frameForce_ = Vector2.zero;

//            float td = Horde.Time.DeltaTime;

//            if (Input.GetKeyDown(KeyCode.R))
//                Global.SceneAccess.CameraShake.AddTrauma(1.0f);

//            if (Input.GetKeyDown(KeyCode.B))
//                ToggleBulletTime();

//            if (Input.GetKeyDown(KeyCode.DownArrow))
//                Fire(Vector2.down);
//            else if (Input.GetKeyDown(KeyCode.UpArrow))
//                Fire(Vector2.up);
//            else if (Input.GetKeyDown(KeyCode.LeftArrow))
//                Fire(Vector2.left);
//            else if (Input.GetKeyDown(KeyCode.RightArrow))
//                Fire(Vector2.right);

//            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.F))
//            {
//                ProjectileSpawners.SpawnCircle(
//                    Global.SceneAccess.ProjectileBlueprints.Bullet2,
//                    collidePlayer: true,
//                    trans_.localPosition + Vector3.down * 0.75f,
//                    radius: 3f,
//                    count: 30,
//                    speed: 2.5f,
//                    Global.SceneAccess.ProjectileManager,
//                    ProjectileUpdaters.ChasePlayer);

//                Global.SceneAccess.CameraShake.AddTrauma(1.0f);
//            }

//            Vector3 inputVec = Vector3.zero;
//            inputVec.x = Input.GetAxis("Horizontal");
//            inputVec.y = Input.GetAxis("Vertical");

//            bool isRunning = inputVec.sqrMagnitude > 0.0f;
//            if (isRunning)
//            {
//                var velocity = inputVec.normalized * td * MoveSpeed;
//                actorBody_.Move(velocity);

//                if (Input.GetMouseButtonDown(1) && !dodge_.IsDodging)
//                    dodge_.DoDodge(inputVec);
//            }

//            PlayerSpriteRenderer.Sprite = SimpleSpriteAnimator.GetAnimationSprite(isRunning ? Anim.Run : Anim.Idle, Anim.DefaultAnimationFramesPerSecond);
//            PlayerSpriteRenderer.FlipX = flipX_;

//            ReportPosition();
//        }
//    }
//}
