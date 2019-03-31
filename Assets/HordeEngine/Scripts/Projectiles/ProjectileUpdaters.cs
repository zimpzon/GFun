//using UnityEngine;

//namespace HordeEngine
//{
//    public static class ProjectileUpdaters
//    {
//        static bool CollidePlayer(Vector2 pos, float size, Vector2 velocity, ref Projectile p)
//        {
//            float s2 = PlayerCollision.PlayerSize + size;
//            if (Mathf.Abs(pos.x - PlayerCollision.PlayerPos.x) < s2 && Mathf.Abs(pos.y - PlayerCollision.PlayerPos.y) < s2)
//            {
//                PlayerCollision.OnPlayerCollision?.Invoke(ref p, velocity);
//                return true;
//            }

//            return false;
//        }

//        public static bool BasicMove(ref Projectile p)
//        {
//            p.ActualPos += p.Velocity * Horde.Time.DeltaSlowableTime;
//            if (p.CollidePlayer && CollidePlayer(p.ActualPos, p.CollisionSize, p.Velocity, ref p))
//                return false;

//            return !CollisionUtil.IsCircleCollidingMap(p.ActualPos, p.CollisionSize);
//        }

//        public static bool CirclingMove(ref Projectile p)
//        {
//            p.Origin += p.Velocity * Horde.Time.DeltaSlowableTime;
//            var oldPos = p.ActualPos;

//            float deg = Horde.Time.SlowableTime * 100 + p.Idx * 5;
//            float sin = Mathf.Sin(-deg * Mathf.Deg2Rad);
//            float cos = Mathf.Cos(-deg * Mathf.Deg2Rad);

//            p.ActualPos.x = p.Origin.x + cos - sin;
//            p.ActualPos.y = p.Origin.y + sin + cos;

//            var move = p.ActualPos - oldPos;
//            p.RotationDegrees = Mathf.Atan2(move.x, move.y) * Mathf.Rad2Deg;
//            if (p.CollidePlayer && CollidePlayer(p.ActualPos, p.CollisionSize, p.Velocity, ref p))
//                return false;

//            return !CollisionUtil.IsCircleCollidingMap(p.ActualPos, p.CollisionSize);
//        }

//        public static bool ChasePlayer(ref Projectile p)
//        {
//            float timeAlive = Horde.Time.SlowableTime - p.StartTime;

//            // Spawn fast -> stop -> start engines
//            const float SpawnTime = 1.0f;

//            float moveSpeed = 0.0f;
//            if (timeAlive < SpawnTime)
//            {
//                // First x seconds: Go from full speed to 0
//                moveSpeed = (SpawnTime - timeAlive) * (1.0f / SpawnTime);
//                moveSpeed *= moveSpeed;
//                moveSpeed *= p.Speed;
//            }
//            else
//            {
//                // After x seconds: speed up to max speed
//                float t = (timeAlive - SpawnTime) * 2;
//                moveSpeed = Mathf.Clamp(t * t * t, 0.0f, p.Speed);
//            }

//            float turnPower = moveSpeed * 0.1f;
//            if (timeAlive < SpawnTime)
//                turnPower = 0.0f;

//            var desiredDir = (PlayerCollision.PlayerPos - p.ActualPos);
//            p.Velocity.x += (desiredDir.x - p.Velocity.x) * Horde.Time.DeltaSlowableTime * turnPower;
//            p.Velocity.y += (desiredDir.y - p.Velocity.y) * Horde.Time.DeltaSlowableTime * turnPower;
//            p.Velocity = p.Velocity.normalized;

//            p.ActualPos += p.Velocity * Horde.Time.DeltaSlowableTime * moveSpeed * p.Speed;
//            p.RotationDegrees = Mathf.Atan2(p.Velocity.x, p.Velocity.y) * Mathf.Rad2Deg;
//            if (p.CollidePlayer && CollidePlayer(p.ActualPos, p.CollisionSize, p.Velocity, ref p))
//            {
//                PlayerCollision.OnPlayerCollision(ref p, p.Velocity);
//                return false;
//            }

//            return !CollisionUtil.IsCircleCollidingMap(p.ActualPos, p.CollisionSize);
//        }

//        public static bool UpdateProjectile2(ref Projectile p)
//        {
//            p.ActualPos += p.Velocity * Horde.Time.DeltaSlowableTime;
//            float dist = (p.ActualPos - p.StartPos).sqrMagnitude;
//            if (dist > p.MaxDist * p.MaxDist)
//                return false;

//            p.Velocity += p.Velocity * 1.5f * Horde.Time.DeltaSlowableTime;
//            return !CollisionUtil.IsCircleCollidingMap(p.ActualPos, p.CollisionSize);
//        }
//    }
//}
