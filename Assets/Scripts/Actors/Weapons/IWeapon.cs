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

        void OnTriggerDown();
        void OnTriggerUp();
        void SetOwnerPhysics(IPhysicsActor actor);
    }
}
