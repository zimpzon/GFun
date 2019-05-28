using GFun;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public WeaponIds WeaponId;

    IWeapon weapon_;
    GameObject weaponObj_;
    InteractableTrigger interact_;

    void Start()
    {
        weaponObj_ = Weapons.Instance.CreateWeapon(WeaponId);
        weaponObj_.transform.SetParent(this.transform);
        weaponObj_.transform.localPosition = Vector3.zero;
        weaponObj_.transform.rotation = Quaternion.Euler(0, 0, Random.value * 360);
        weapon_ = weaponObj_.GetComponent<IWeapon>();
        interact_ = GetComponentInChildren<InteractableTrigger>();
        interact_.Message = weapon_.Name;
        interact_.OnAccept.AddListener(OnPickup);

        var editorSprite = GetComponent<SpriteRenderer>();
        editorSprite.enabled = false;
    }

    public void OnPickup()
    {
        var player = PlayableCharacters.GetPlayerInScene();
        player.AttachWeapon(weaponObj_);
        this.gameObject.SetActive(false);
    }
}
