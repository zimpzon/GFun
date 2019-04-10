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
        void SetOwnerPhysics(IPhysicsActor actor);
    }
}
