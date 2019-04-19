using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInteractScript : MonoBehaviour
{
    public static PlayerInteractScript Instance;

    bool isShown_;
    UnityEvent onAccept_;
    TextMeshProUGUI text_;
    Transform transform_;
    Transform target_;
    Vector3 offset_;
    Camera mainCam_;
    Canvas parentCanvas_;

    private void Awake()
    {
        Instance = this;
        transform_ = transform;
        mainCam_ = Camera.main;
        text_ = GetComponentInChildren<TextMeshProUGUI>();
        parentCanvas_ = GetComponentInChildren<Canvas>();

        Hide();
    }

    public void Show(Transform target, Vector3 offset, string message, UnityEvent onAccept)
    {
        string msg = $"{message}{Environment.NewLine}(<color=#00ff00>E</color>)";
        text_.text = msg;
        target_ = target;
        offset_ = offset;
        onAccept_ = onAccept;

        SetPosition();
        this.gameObject.SetActive(true);
        isShown_ = true;
    }

    public Vector3 worldToUISpace(Canvas parentCanvas, Vector3 worldPos)
    {
        //Convert the world for screen point so that it can be used with ScreenPointToLocalPointInRectangle function
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        Vector2 movePos;

        //Convert the screenpoint to ui rectangle local point
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvas.transform as RectTransform, screenPos, parentCanvas.worldCamera, out movePos);
        //Convert the local point to world point
        return parentCanvas.transform.TransformPoint(movePos);
    }

    void SetPosition()
    {
        var worldPos = target_.position + offset_;
        text_.transform.position = worldToUISpace(parentCanvas_, worldPos);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
        isShown_ = false;
    }

    private void Update()
    {
        if (isShown_)
        {
            SetPosition();

            if (Input.GetKeyDown(KeyCode.E))
            {
                Hide();
                onAccept_?.Invoke();
            }
        }
    }
}
