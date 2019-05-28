using GFun;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public WeaponIds WeaponId;
    public SpriteRenderer WeaponSprite;

    IWeapon weapon_;
    GameObject weaponObj_;
    InteractableTrigger interact_;

    void Start()
    {
        weaponObj_ = Weapons.Instance.CreateWeapon(WeaponId);
        weaponObj_.transform.SetParent(this.transform);
        weaponObj_.transform.localPosition = Vector3.zero;
        weaponObj_.transform.rotation = Quaternion.Euler(0, 0, Random.value * 360);
        weaponObj_.SetActive(false);
        weapon_ = weaponObj_.GetComponent<IWeapon>();

        interact_ = GetComponentInChildren<InteractableTrigger>();
        interact_.Message = weapon_.Name;
        interact_.OnAccept.AddListener(OnPickup);

        var weaponSprite = GetComponent<SpriteRenderer>();
        WeaponSprite.sprite = weaponObj_.GetComponentInChildren<SpriteRenderer>().sprite;
        WeaponSprite.transform.rotation = Quaternion.Euler(0, 0, Random.value * 360);
    }

    public void OnPickup()
    {
        var player = PlayableCharacters.GetPlayerInScene();
        weaponObj_.SetActive(true);
        player.AttachWeapon(weaponObj_);
        this.gameObject.SetActive(false);
    }
}
