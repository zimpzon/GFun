using UnityEngine;

public class MiniMapCamera : MonoBehaviour
{
    public static MiniMapCamera Instance;

    public bool IsShown;
    public float Z;

    Camera camera_;
    Transform transform_;

    public void Show(bool show)
    {
        camera_.enabled = show;
        IsShown = show;
    }

    private void Awake()
    {
        Instance = this;

        transform_ = transform;
        camera_ = GetComponent<Camera>();
    }

    void Update()
    {
        var player = PlayableCharacters.GetPlayerInScene();
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (player == null || player.IsDead)
                return;

            Show(!IsShown);
        }

        if (IsShown)
        {
            var pos = player.transform.position;
            pos.z = Z;
            transform.position = pos;
        }
    }
}
