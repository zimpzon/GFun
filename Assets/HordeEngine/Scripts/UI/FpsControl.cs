using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FpsControl : MonoBehaviour
{
    public KeyCode ToggleKey = KeyCode.I;
    public bool StartEnabled = true;

    const int W = 128;
    const int H = 64;

    int currentColumn_;
    int textUpdateRate_ = 2;
    Color32 background_ = new Color32(50, 50, 50, 255);
    RawImage image_;
    Texture2D texture_;
    Color32[] pixels_;
    Rect uvRect_ = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
    GameObject displayRoot_;

    private void Awake()
    {
        image_ = GetComponentInChildren<RawImage>();

        pixels_ = new Color32[W * H];
        for (int i = 0; i < pixels_.Length; ++i)
            pixels_[i] = background_;

        texture_ = new Texture2D(W, H, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Repeat
        };

        image_.texture = texture_;
        image_.uvRect = uvRect_;

        displayRoot_ = transform.GetChild(0).gameObject;
        if (!StartEnabled)
            displayRoot_.SetActive(false);
    }

    public void Add(float fps)
    {
        Color col = new Color(0.0f, 0.4f, 0.0f, 1.0f);
        float score = Mathf.Clamp(fps / 60, 0.0f, 1.0f);
        float score2 = score * score;
        col.g = 0.4f * score;
        col.r = (1.0f - score2);
        Color32 col32 = col;
        int top = Mathf.Min(H - 1, Mathf.RoundToInt(fps));
        for (int y = 0; y < H; ++y)
        {
            pixels_[y * W + currentColumn_] = y < top ? col32 : background_;
        }

        // Dashed line
        int row60Fps = 60;
        if ((currentColumn_ / 5) % 2 == 0)
            pixels_[row60Fps * W + currentColumn_] = Color.green;

        texture_.SetPixels32(pixels_);
        texture_.Apply();

        uvRect_.x = (1.0f / W) * (currentColumn_ + 1);
        image_.uvRect = uvRect_;

        if (++currentColumn_ >= W)
            currentColumn_ = 0;
    }

    void Update()
    {
        if (Input.GetKeyDown(ToggleKey))
            displayRoot_.SetActive(!displayRoot_.activeSelf);

        if (displayRoot_.activeSelf)
        {
            float fps = 1.0f / Time.deltaTime;
            Add(fps);
        }
    }
}
