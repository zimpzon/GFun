using MEC;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerInfoScript : MonoBehaviour
{
    class Item
    {
        public string info;
        public Color color;
    }

    public static PlayerInfoScript Instance;

    RectTransform transform_;
    TextMeshProUGUI text_;
    Queue<Item> queue_ = new Queue<Item>();

    private void Awake()
    {
        Instance = this;
        transform_ = GetComponent<RectTransform>();
        text_ = GetComponentInChildren<TextMeshProUGUI>();
        transform_.localPosition += Vector3.right * 1000;
        Timing.RunCoroutine(QueueCo().CancelWith(this.gameObject));
    }

    public void ShowInfo(string info, Color color)
    {
        queue_.Enqueue(new Item { info = info, color = color });
    }

    IEnumerator<float> QueueCo()
    {
        while (true)
        {
            if (queue_.Count == 0)
            {
                yield return 0;
                continue;
            }
            // TODO: Disappears?
            var item = queue_.Dequeue();
            text_.text = item.info;
            text_.color = item.color;

            var pos = transform_.localPosition;
            float t = 1.0f;
            const float JustOutside = -800;
            while (t > 0.0f)
            {
                float sqr = t * t;
                pos = transform_.localPosition;
                pos.x = sqr * JustOutside;
                transform_.localPosition = pos;

                t -= Time.unscaledDeltaTime * 8;
                yield return 0;
            }

            pos.x = 0;
            transform_.localPosition = pos;

            yield return Timing.WaitForSeconds(4);

            t = 0;
            while (t < 1.0f)
            {
                float sqr = t * t;
                pos.x = sqr * -JustOutside;
                transform_.localPosition = pos;

                t += Time.unscaledDeltaTime * 8;
                yield return 0;
            }

            transform_.localPosition += Vector3.right * 1000;
        }
    }
}
