//using UnityEngine;

//namespace HordeEngine
//{
//    [RequireComponent(typeof(ActorPhysicsBody))]
//    public class ActorAbility_FireCircle : MonoBehaviour, IComponentUpdate
//    {
//        ActorPhysicsBody actorBody_;
//        float nextFire_;

//        private void Awake()
//        {
//            actorBody_ = GetComponent<ActorPhysicsBody>();
//        }

//        void OnEnable() { Horde.ComponentUpdater.RegisterForUpdate(this, ComponentUpdatePass.Default); }
//        void OnDisable() { Horde.ComponentUpdater.UnregisterForUpdate(this, ComponentUpdatePass.Default); }

//        bool Filter(ref Projectile p)
//            => p.RotationDegrees < 45 || p.RotationDegrees > 135;

//        public void ComponentUpdate(ComponentUpdatePass pass)
//        {
//            if (Horde.Time.SlowableTime > nextFire_)
//            {
//                nextFire_ = Horde.Time.SlowableTime + 2 + Random.value * 1;

//                ProjectileSpawners.SpawnCircle(
//                    Global.SceneAccess.ProjectileBlueprints.Bullet0,
//                    collidePlayer: true,
//                    actorBody_.Position,
//                    radius: 1.5f,
//                    count: Random.Range(20, 80),
//                    speed: Random.value * 6 + 4,
//                    Global.SceneAccess.ProjectileManager,
//                    ProjectileUpdaters.BasicMove,
//                    Filter);
//            }
//        }
//    }
//}
