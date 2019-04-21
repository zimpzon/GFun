using TMPro;
using UnityEngine;

public class CoinWidgetScript : MonoBehaviour
{
    public static CoinWidgetScript Instance;

    TextMeshProUGUI text_;

    public void SetAmount(int amount)
    {
        text_.text = amount.ToString("000");
    }

    private void Awake()
    {
        Instance = this;
        text_ = GetComponentInChildren<TextMeshProUGUI>();
    }
}
