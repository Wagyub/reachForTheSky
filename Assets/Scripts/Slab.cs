using UnityEngine;

public class Slab : MonoBehaviour
{
    private void OnMouseDown()
    {
        transform.parent.SendMessage("OnMouseDown", SendMessageOptions.DontRequireReceiver);
    }

    private void OnMouseEnter()
    {
        transform.parent.SendMessage("OnMouseEnter", SendMessageOptions.DontRequireReceiver);
    }

    private void OnMouseExit()
    {
        transform.parent.SendMessage("OnMouseExit", SendMessageOptions.DontRequireReceiver);
    }
}