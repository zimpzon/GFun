using GFun;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public WeaponIds WeaponId;
    public SpriteRenderer WeaponSprite;

    IWeapon weapon_;
    GameObject weaponObj_;
    InteractableTrigger interact_;

    public void Throw(Vector3 force)
    {
        var body = GetComponent<Rigidbody2D>();
        body.AddForce(force, ForceMode2D.Impulse);
    }

    public void CreateFromExisting(GameObject weapon)
    {
        weaponObj_ = weapon;
        weapon_ = weaponObj_.GetComponent<IWeapon>();
        WeaponId = weapon_.Id;
    }

    void Start()
    {
        if (weaponObj_ == null)
        {
            weaponObj_ = Weapons.Instance.CreateWeapon(WeaponId);
            weapon_ = weaponObj_.GetComponent<IWeapon>();
        }

        weaponObj_.transform.SetParent(this.transform);
        weaponObj_.transform.localScale = Vector3.one;
        weaponObj_.transform.localPosition = Vector3.zero;
        weaponObj_.transform.rotation = Quaternion.Euler(0, 0, Random.value * 360);
        weaponObj_.SetActive(false);

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
        FloatingTextSpawner.Instance.Spawn(player.transform.position + Vector3.up * 0.5f, weapon_.Name, Color.white);
        player.EquipWeapon(weaponObj_);
        Destroy(this.gameObject);
    }
}
