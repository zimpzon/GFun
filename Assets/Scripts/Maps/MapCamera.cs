using UnityEngine;

public class MapCamera : MonoBehaviour
{
    public float Z;

    Camera camera_;
    Transform transform_;

    private void Awake()
    {
        transform_ = transform;
        camera_ = GetComponent<Camera>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            camera_.enabled = !camera_.enabled;
        }

        if (camera_.enabled)
        {
            var pos = SceneGlobals.Instance.PlayerScript.transform.position;
            pos.z = Z;
            transform.position = pos;
        }
    }
}
