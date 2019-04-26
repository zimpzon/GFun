using UnityEngine;
using UnityEngine.UI;

public class EnergyWidget : MonoBehaviour
{
    public static EnergyWidget Instance;

    public RawImage EnergyBar;

    float energyBarMaxWidth_;
    float energyBarMaxHeight_;

    private void Awake()
    {
        Instance = this;

        energyBarMaxWidth_ = EnergyBar.rectTransform.rect.width;
        energyBarMaxHeight_ = EnergyBar.rectTransform.rect.height;
    }

    public void ShowEnergy(int current, int max)
    {
        EnergyBar.rectTransform.sizeDelta = new Vector2((energyBarMaxWidth_ / max) * current, energyBarMaxHeight_);
    }
}
