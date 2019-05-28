using GFun;
using UnityEngine;

public class NullWeapon : MonoBehaviour, IWeapon
{
    public WeaponIds Id => WeaponIds.NullWeapon;
    public string Name => "No Weapon";
    public int Level => 0;
    public AmmoType AmmoType => AmmoType.None;
    public int AmmoCount => 0;

    public Vector3 GetMuzzlePosition(Vector3 _) => Vector3.right;
    public Vector3 LatestFiringDirection => Vector3.right;
    public float LatestFiringTimeUnscaled => float.MinValue;

    public void OnTriggerDown(Vector3 _) { }
    public void OnTriggerUp() { }

    public void SetOwner(IPhysicsActor _) { }
    public void SetAmmoProvider(IAmmoProvider _) { }
}
