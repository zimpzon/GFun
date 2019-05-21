using UnityEngine;

public class LockedCharacterScript : MonoBehaviour
{
    SpriteAnimator_IdleRun animator_;

    private void Start()
    {
        animator_ = GetComponentInChildren<SpriteAnimator_IdleRun>();

    }
    public void OnInteract()
    {
        PlayerInfoScript.Instance.ShowInfo("Your Hand Goes Right Through");
    }

    void Update()
    {
        animator_.UpdateAnimation(Vector3.left);
    }
}
