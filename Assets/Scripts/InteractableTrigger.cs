using UnityEngine;
using UnityEngine.Events;

public class InteractableTrigger : MonoBehaviour
{
    public string Message;
    public Vector3 MessageOffset;
    public UnityEvent OnAccept;

    public PortalScript PortalScript;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerInteractScript.Instance.Show(transform, MessageOffset, Message, OnAccept);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        PlayerInteractScript.Instance.Hide();
    }
}
