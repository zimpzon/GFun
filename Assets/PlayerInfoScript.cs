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

    Transform transform_;
    TextMeshProUGUI text_;
    Queue<Item> queue_ = new Queue<Item>();
    Vector3 basePosition_;

    private void Awake()
    {
        Instance = this;
        transform_ = transform;
        text_ = GetComponentInChildren<TextMeshProUGUI>();
        basePosition_ = transform_.position;
        basePosition_.x = 10000;
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

            var item = queue_.Dequeue();
            text_.text = item.info;
            text_.color = item.color;

            var pos = basePosition_;
            float t = 0.0f;
            const float JustOutside = -800;
            while (t < 1.0f)
            {
                pos.x = JustOutside + (t * -JustOutside);
                transform_.position = pos;

                t += Time.unscaledDeltaTime;
                yield return 0;
            }
        }
    }
}
