using UnityEngine;

namespace GFun
{
    public interface IWeapon
    {
        WeaponIds Id { get; }
        string Name { get; }
        AmmoType AmmoType { get; }
        int AmmoMax { get; }
        int AmmoCount { get; set; }
        int Level { get; }

        Vector3 GetMuzzlePosition(Vector3 target);
        void OnTriggerDown(Vector3 firingDirection);
        void OnTriggerUp();
        Vector3 LatestFiringDirection { get; }
        float LatestFiringTimeUnscaled { get; }
        void SetOwner(IPhysicsActor forceReceiver);
    }
}
