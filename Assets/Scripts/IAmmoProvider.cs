using GFun;

public interface IAmmoProvider
{
    bool TryUseAmmo(AmmoType ammoType, int amount);
    int GetCurrentAmount(AmmoType ammoType);
}
