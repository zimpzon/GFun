//using UnityEngine;

//namespace HordeEngine
//{
//    [RequireComponent(typeof(ActorPhysicsBody))]
//    public class ActorAbility_FireRockets : MonoBehaviour, IComponentUpdate
//    {
//        ActorPhysicsBody actorBody_;
//        float nextFire_;

//        private void Awake()
//        {
//            actorBody_ = GetComponent<ActorPhysicsBody>();
//        }

//        void OnEnable() { Horde.ComponentUpdater.RegisterForUpdate(this, ComponentUpdatePass.Default); }
//        void OnDisable() { Horde.ComponentUpdater.UnregisterForUpdate(this, ComponentUpdatePass.Default); }

//        public void ComponentUpdate(ComponentUpdatePass pass)
//        {
//            if (Horde.Time.SlowableTime > nextFire_)
//            {
//                nextFire_ = Horde.Time.SlowableTime + 5 + Random.value * 3;

//                ProjectileSpawners.SpawnCircle(
//                    Global.SceneAccess.ProjectileBlueprints.Bullet2,
//                    collidePlayer: true,
//                    actorBody_.Position,
//                    radius: 0.5f,
//                    count: 3,
//                    speed: 2.5f,
//                    Global.SceneAccess.ProjectileManager,
//                    ProjectileUpdaters.ChasePlayer);
//            }
//        }
//    }
//}
