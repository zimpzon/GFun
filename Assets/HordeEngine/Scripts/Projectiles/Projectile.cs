using UnityEngine;

namespace HordeEngine
{
    public struct Projectile
    {
        public void Reset()
        {
            Idx = 0;
            StartTime = Horde.Time.SlowableTime;
            StartPos = Vector2.zero;
            Origin = Vector2.zero;
            OriginOffset = Vector2.zero;
            MaxDist = float.MaxValue;
            MaxTime = float.MaxValue;

            Sprite = null;
            Material = null;
            Size = Vector2.one;
            Color = new Color32(255, 255, 255, 255);
            EmitLight = false;
            LightSprite = null;
            LightMaterial = null;
            LightSize = Vector2.one;
            LightColor = new Color32(255, 255, 255, 255);
            LightOffsetY = 0.0f;

            CollidePlayer = false;
            CollisionSize = 1.0f;
            BounceWalls = false;
            BouncesLeft = 0;

            Velocity = Vector2.zero;
            ActualPos = Vector2.zero;
            RotationDegrees = 0;
            Z = 0;

            UpdateCallback = null;
        }

        public void ApplyBlueprint(ProjectileBlueprint desc)
        {
            Sprite = desc.Sprite;
            Material = desc.Material;
            Size = desc.Size;
            Color = desc.Color;

            EmitLight = desc.EmitLight;
            LightSprite = desc.LightSprite;
            LightMaterial = desc.LightMaterial;
            LightSize = desc.LightSize;
            LightColor = desc.LightColor;
            LightOffsetY = desc.LightOffsetY;

            CollisionSize = desc.CollisionSize;
            BounceWalls = desc.BounceWalls;
            BouncesLeft = desc.MaxWallBounces;

            MaxDist = desc.MaxDistance;
            MaxTime = desc.MaxTime;
        }

        public int Idx;
        public float StartTime;
        public Vector2 StartPos;
        public Vector2 Origin;
        public Vector2 OriginOffset;
        public float MaxDist;
        public float MaxTime;

        public Sprite Sprite;
        public Material Material;
        public Vector2 Size;
        public Color32 Color;
        public bool EmitLight;
        public Sprite LightSprite;
        public Material LightMaterial;
        public Vector2 LightSize;
        public Color LightColor;
        public float LightOffsetY;

        public bool CollidePlayer;
        public float CollisionSize;
        public bool BounceWalls;
        public int BouncesLeft;

        public float Speed;
        public Vector2 Velocity;
        public Vector2 ActualPos;
        public float RotationDegrees;
        public float Z;
        
        public delegate bool TickDelegate(ref Projectile projectile);
        public TickDelegate UpdateCallback;
    }
}
