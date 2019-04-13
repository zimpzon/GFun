using GFun;
using UnityEngine;

public class NullWeapon : MonoBehaviour, IWeapon
{
    public WeaponIds Id => WeaponIds.NullWeapon;
    public string Name => "No Weapon";
    public int Level => 0;
    public AmmoType AmmoType => AmmoType.None;
    public int AmmoCount => 0;
    public int AmmoMax => 0;
    public void OnTriggerUp() { }
    public void OnTriggerDown() { }
    public void SetOwnerPhysics(IPhysicsActor actor) { }
}
