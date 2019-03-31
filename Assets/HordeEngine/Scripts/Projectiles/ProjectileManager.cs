using System;
using HordeEngine;
using UnityEngine;

public class ProjectileManager : MonoBehaviour, IComponentUpdate
{
    public int InitialCapacity = 5000;
    public float OffsetY;
    [SerializeField, Layer] public LayerMask ProjectileLayer;
    [SerializeField, Layer] public LayerMask LightLayer;

    // To allow updating structs in place we have to use array. A List<> will return a copy when accessing an element.
    Projectile[] projectiles_;
    int projectileLayer_;
    int lightLayer_;

    [Header("Debug")]
    public int ActiveProjectiles;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        for (int i = 0; i < ActiveProjectiles; ++i)
        {
            var p = projectiles_[i];
            var pos = p.ActualPos;
            pos.y += OffsetY;
            Gizmos.DrawWireSphere(pos, p.CollisionSize);
        }
    }

    void Awake()
    {
        projectiles_ = new Projectile[InitialCapacity];

        projectileLayer_ = (int)ProjectileLayer;
        lightLayer_ = (int)LightLayer;
    }

    public void Clear()
    {
        ActiveProjectiles = 0;
    }

    public void SpawnProjectile(ref Projectile p)
    {
        projectiles_[ActiveProjectiles++] = p;

        if (ActiveProjectiles == projectiles_.Length)
        {
            Array.Resize(ref projectiles_, projectiles_.Length + (projectiles_.Length / 2));
            Debug.Log("Projectile array was expanded. Consider increasing initial capacity. New size: " + projectiles_.Length);
        }
    }

    void RenderProjectile(ref Projectile p)
    {
        Vector3 pos = p.ActualPos;
        pos.z = p.Z;
        pos.y += OffsetY;
        Horde.Sprites.AddQuad(pos, p.Size, p.RotationDegrees, p.Size.y, p.Color, p.Sprite, p.Material, projectileLayer_);

        if (p.EmitLight)
        {
            pos.y += p.LightOffsetY;
            Horde.Sprites.AddQuad(pos, p.LightSize, p.RotationDegrees, p.LightSize.y, p.LightColor, p.LightSprite, p.LightMaterial, lightLayer_);
        }
    }

    void OnEnable() { Horde.ComponentUpdater.RegisterForUpdate(this, ComponentUpdatePass.Internal_DrawMeshes); }
    void OnDisable() { Horde.ComponentUpdater.UnregisterForUpdate(this, ComponentUpdatePass.Internal_DrawMeshes); }

    public void ComponentUpdate(ComponentUpdatePass pass)
    {
        int i = 0;
        while (i < ActiveProjectiles)
        {
            bool success = projectiles_[i].UpdateCallback(ref projectiles_[i]);
            if (!success)
            {
                // Overwrite current with bottom of list and rerun current (i not incremented)
                projectiles_[i] = projectiles_[ActiveProjectiles - 1];
                ActiveProjectiles--;
                continue;
            }

            RenderProjectile(ref projectiles_[i]);

            i++;
        }
    }
}
