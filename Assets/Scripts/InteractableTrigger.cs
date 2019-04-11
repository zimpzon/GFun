using UnityEngine;
using UnityEngine.Events;

public class InteractableTrigger : MonoBehaviour
{
    public string Message;
    public Vector3 MessageOffset;
    public UnityEvent OnAccept;

    Collider2D collider_;

    private void Awake()
    {
        collider_ = GetComponent<Collider2D>();
    }

    public void EnableTrigger(bool enable)
    {
        collider_.enabled = enable;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerInteractScript.Instance.Show(transform, MessageOffset, Message, OnAccept);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        PlayerInteractScript.Instance.Hide();
    }
}
