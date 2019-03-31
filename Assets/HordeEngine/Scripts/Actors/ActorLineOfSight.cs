//using System;
//using UnityEngine;

//namespace HordeEngine
//{
//    [RequireComponent(typeof(ActorPhysicsBody))]
//    public class ActorLineOfSight : MonoBehaviour, IComponentUpdate
//    {
//        public ActorPhysicsBody Target;
//        public float RequiredWidth = 0.1f;
//        [NonSerialized] public bool HasLineOfSight;
//        [NonSerialized] public Vector2 LatestLineOfSightPosition;
//        [NonSerialized] public float LatestLineOfSightTime;
//        public float LatestLineOfSightAge => Horde.Time.SlowableTime - LatestLineOfSightTime;

//        static int IdCounter = AiBlackboard.LineOfSightFrameSkipOffset;
//        readonly int myId_ = IdCounter++;

//        ActorPhysicsBody body_;

//        void Awake()
//        {
//            body_ = GetComponent<ActorPhysicsBody>();
//        }

//        public bool TryGetRecentLineOfSightPosition(out Vector2 pos, float maxAge)
//        {
//            if (LatestLineOfSightTime >= Horde.Time.Time - maxAge)
//            {
//                pos = LatestLineOfSightPosition;
//                return true;
//            }

//            pos = Vector2.zero;
//            return false;
//        }

//        void OnEnable() { Horde.ComponentUpdater.RegisterForUpdate(this, ComponentUpdatePass.Default); }
//        void OnDisable() { Horde.ComponentUpdater.UnregisterForUpdate(this, ComponentUpdatePass.Default); }

//        public void ComponentUpdate(ComponentUpdatePass pass)
//        {
//            bool checkNow = (Time.frameCount + myId_) % AiBlackboard.LineOfSightFrameSkip == 0;
//            if (checkNow)
//            {
//                var p0 = body_.Position;
//                var p1 = Target.Position;

//                HasLineOfSight = CollisionUtil.CircleCast(p0, p1, RequiredWidth, allowPartial: true);
//                if (HasLineOfSight)
//                {
//                    LatestLineOfSightPosition = p1;
//                    LatestLineOfSightTime = Horde.Time.Time;
//                }

//                Debug.DrawLine(p0, LatestLineOfSightPosition, HasLineOfSight ? Color.green : Color.yellow, 0.32f);
//            }
//        }
//    }
//}
