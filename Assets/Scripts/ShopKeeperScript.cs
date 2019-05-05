using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ShopItemId
{
    Item1Health = 1,
    IncreaseMaxHp1 = 2,
}

public class ShopItem
{
    public ShopItemId Id;
    public string ShopText;
    public int Price;
    public int Level;

    public static ShopItem Create(ShopItemId item, string shopText, int price, int level)
        => new ShopItem { Id = item, ShopText = shopText, Price = price, Level = level };
}

public class ShopKeeperScript : MonoBehaviour
{
    public static int Price1Health = 100;

    public GameObject[] Scythes;

    public AudioClip DenyBuySound;
    public AudioClip AcceptBuySound;
    public AudioClip ShopKeeperAggroSound;

    public GameObject[] Displays;

    bool scythesEnabled_;

    List<ShopItem> allItems_ = new List<ShopItem>();

    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnPoke();    
    }

    public void OnPoke()
    {
        if (scythesEnabled_)
            return;
        CurrentRunData.Instance.ShopKeeperPokeCount++;
        int scytheCount = CurrentRunData.Instance.ShopKeeperPokeCount + 1;
        if (scytheCount > Scythes.Length)
            scytheCount = Scythes.Length;

        for (int i = 0; i < scytheCount; ++i)
        {
            if (i != 0 && Random.value < 0.1f)
                continue;

            var scythe = Scythes[i];
            Vector3 offset = Random.insideUnitCircle * 1.0f;
            scythe.transform.position += offset;
            scythe.SetActive(true);
            ParticleScript.EmitAtPosition(ParticleScript.Instance.PlayerLandParticles, scythe.transform.position, 5);
        }
        AudioManager.Instance.PlaySfxClip(ShopKeeperAggroSound, 1);

        scythesEnabled_ = true;
    }

    void ExecuteBuy(ShopItemId id, int displayIdx)
    {
        var item = allItems_.Where(i => i.Id == id).Single();
        if (!AcceptBuy(item.Price))
            return;

        var player = PlayableCharacters.GetPlayerInScene();

        switch (id)
        {
            case ShopItemId.Item1Health:
                bool isOverheal = player.Life == player.MaxLife;
                if (isOverheal)
                    FloatingTextSpawner.Instance.Spawn(transform.position + Vector3.up * 0.5f, "Hah!", Color.white);
                player.AddHealth(1, "Shop Item");
                break;
            case ShopItemId.IncreaseMaxHp1:
                player.AddMaxHealth(1);
                break;
           default:
                print("Not implemented: " + id);
                break;
        }

        CurrentRunData.Instance.ItemsBought++;

        var display = Displays[displayIdx];
        ParticleScript.EmitAtPosition(ParticleScript.Instance.PlayerLandParticles, display.transform.position, 5);
        ParticleScript.EmitAtPosition(ParticleScript.Instance.MuzzleFlashParticles, display.transform.position, 1);
        display.SetActive(false);
        GameSceneLogic.Instance.UpdateCoinWidget();
    }

    SpriteAnimator_Single anim_;

    bool AcceptBuy(int price)
    {
        if (CurrentRunData.Instance.Coins >= price)
        {
            CurrentRunData.Instance.Coins -= price;
            AudioManager.Instance.PlaySfxClip(AcceptBuySound, 2);
            AudioManager.Instance.PlaySfxClip(AcceptBuySound, 2);
            return true;
        }
        else
        {
            AudioManager.Instance.PlaySfxClip(DenyBuySound, 1);
            return false;
        }
    }

    void Start()
    {
        anim_ = GetComponentInChildren<SpriteAnimator_Single>();

        CreateItemData();
        InitDisplays(level: 1);
    }

    List<ShopItem> GetRandomItems(int count, int level)
    {
        var result = new List<ShopItem>();
        var selection = allItems_.Where(item => item.Level >= level).ToList();

        while (selection.Count < count)
            selection.Add(allItems_[0]);

        for (int i = 0; i < count; ++i)
        {
            int idx = Random.Range(0, selection.Count);
            var item = selection[idx];
            result.Add(item);
            selection.Remove(item);
        }
        return result;
    }

    void InitDisplays(int level)
    {
        int displayCount = Displays.Length;
        var items = GetRandomItems(displayCount, level);

        for (int i = 0; i < displayCount; ++i)
        {
            int idx = i; // https://blogs.msdn.microsoft.com/ericlippert/2009/11/12/closing-over-the-loop-variable-considered-harmful/
            var display = Displays[i];
            display.SetActive(true);
            var interact = display.GetComponentInChildren<InteractableTrigger>();
            var item = items[i];
            interact.Message = $"{item.ShopText} [<color=#ffff00>${item.Price}</color>]";
            interact.OnAccept.AddListener(delegate { ExecuteBuy(item.Id, idx); });
        }
    }

    void Update()
    {
        anim_.UpdateAnimation(Vector3.zero);
    }

    void CreateItemData()
    {
        allItems_.Clear();
        allItems_.Add(ShopItem.Create(ShopItemId.Item1Health, "Heal 1 HP", 25, 1));
        allItems_.Add(ShopItem.Create(ShopItemId.IncreaseMaxHp1, "Increase max HP by 1", 50, 1));
    }
}
