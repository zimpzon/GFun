using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthWidget : MonoBehaviour
{
    public static HealthWidget Instance;

    public TextMeshProUGUI TextLife;
    public RawImage LifeBar;

    float lifeBarMaxWidth_;
    float lifeBarMaxHeight_;
    bool doWarn_;
    private void Awake()
    {
        Instance = this;

        lifeBarMaxWidth_ = LifeBar.rectTransform.rect.width;
        lifeBarMaxHeight_ = LifeBar.rectTransform.rect.height;
    }

    public void ShowLife(int current, int max)
    {
        LifeBar.rectTransform.sizeDelta = new Vector2((lifeBarMaxWidth_ / max) * current, lifeBarMaxHeight_);
        TextLife.text = $"{current}/{max}";
    }
}
