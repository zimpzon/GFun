using UnityEngine;

namespace GFun
{
    public interface IWeapon
    {
        WeaponIds Id { get; }
        string Name { get; }
        AmmoType AmmoType { get; }
        int AmmoMax { get; }
        int AmmoCount { get; }
        int Level { get; }

        void OnTriggerDown(Vector3 firingDirection);
        void OnTriggerUp();
        Vector3 LatestFiringDirection { get; }
        float LatestFiringTimeUnscaled { get; }
        void SetForceReceiver(IPhysicsActor forceReceiver);
    }
}
