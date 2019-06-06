using UnityEngine;

public class CursorScript : MonoBehaviour
{
    Transform transform_;
    Camera mainCam_;

    void Start()
    {
        transform_ = transform;
        mainCam_ = Camera.main;
        Cursor.lockState = CursorLockMode.Confined;
    }

    void Update()
    {
        Cursor.visible = false;
        //var mouseScreenPos = Input.mousePosition;
        //mouseScreenPos.z = -mainCam_.transform.position.z;
        //var mouseWorldPos = mainCam_.ScreenToWorldPoint(mouseScreenPos);
        //mouseWorldPos.z = 0;

        transform.position = Input.mousePosition;
    }
}
