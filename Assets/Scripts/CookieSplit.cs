using UnityEngine;

public class CookieSplit : MonoBehaviour
{
    public Transform leftHalf;
    public Transform rightHalf;

    private float closedX = 0.46f;
    private float openX = 1.5f;

    [ContextMenu("Test Open Cookie")]
    public void OpenCookie()
    {
        leftHalf.position = new Vector3(-openX, leftHalf.position.y, 0);
        rightHalf.position = new Vector3(openX, rightHalf.position.y, 0);
    }
}