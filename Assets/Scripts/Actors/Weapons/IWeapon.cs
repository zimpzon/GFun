using System;
using UnityEngine;

namespace GFun
{
    public interface IWeapon
    {
        AmmoType AmmoType { get; }
        int AmmoMax { get; }
        int AmmoCount { get; }
        int Level { get; }

        void OnTriggerDown();
        void OnTriggerUp();
        void SetForceCallback(Action<Vector3> forceCallback);
    }
}
