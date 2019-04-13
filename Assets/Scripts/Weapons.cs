using GFun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WeaponLookup
{
    public IWeapon Weapon;
    public GameObject Prefab;
}

public class Weapons : MonoBehaviour
{
    public static Weapons Instance;
    public WeaponPrefabList WeaponPrefabList;
    public List<WeaponLookup> WeaponLookup;

    public List<WeaponLookup> FindWeapons(int minLevel, int maxLevel)
    {
        return WeaponLookup.Where(l => l.Weapon.Level >= minLevel && l.Weapon.Level <= maxLevel).ToList();
    }

    public GameObject CreateWeapon(WeaponIds weaponId)
    {
        var lookup = WeaponLookup.Where(l => l.Weapon.Id == weaponId).Single();
        return CreateWeapon(lookup.Prefab);
    }

    public GameObject CreateWeapon(GameObject prefab)
    {
        var instance = Instantiate(prefab);
        return instance;
    }

    private void Start()
    {
        Instance = this;
        WeaponLookup = WeaponPrefabList.WeaponPrefabs.Select(prefab =>
            new WeaponLookup { Weapon = prefab.GetComponent<IWeapon>(), Prefab = prefab }).ToList();
    }
}
