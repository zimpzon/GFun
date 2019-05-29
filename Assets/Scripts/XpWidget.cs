using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class XpWidget : MonoBehaviour
{
    public static XpWidget Instance;

    public RawImage XpBar;
    public TextMeshProUGUI Text;

    float xpBarMaxWidth_;
    float xpBarMaxHeight_;

    private void Awake()
    {
        Instance = this;

        xpBarMaxWidth_ = XpBar.rectTransform.rect.width;
        xpBarMaxHeight_ = XpBar.rectTransform.rect.height;
    }

    public void ShowXp(int level, int current, int max, int totalXp)
    {
        XpBar.rectTransform.sizeDelta = new Vector2((xpBarMaxWidth_ / max) * current, xpBarMaxHeight_);
        Text.SetText("LVL {0} ({1}/{2})", level, current, max);
    }
}
