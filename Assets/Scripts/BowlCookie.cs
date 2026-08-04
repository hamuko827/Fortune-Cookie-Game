using UnityEngine;

public class BowlCookie : MonoBehaviour
{
    public Vector3 centerPosition = new Vector3(0, 0, 0);
    public Vector3 centerScale = new Vector3(1.5f, 1.5f, 1.5f);
    public float moveSpeed = 5f;

    private bool isSelected = false;
    private bool hasArrived = false;

    public static bool cookieAlreadyChosen = false;

    void OnMouseDown()
    {
        if (cookieAlreadyChosen) return;
        cookieAlreadyChosen = true;
        isSelected = true;
    }

    void Update()
    {
        if (isSelected && !hasArrived)
        {
            transform.position = Vector3.Lerp(transform.position, centerPosition, moveSpeed * Time.deltaTime);
            transform.localScale = Vector3.Lerp(transform.localScale, centerScale, moveSpeed * Time.deltaTime);

            Quaternion targetRotation = Quaternion.Euler(0, 0, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, moveSpeed * Time.deltaTime);

            // check distance to center to know when it's "close enough"
            if (Vector3.Distance(transform.position, centerPosition) < 0.01f)
            {
                hasArrived = true;
                OnArriveAtCenter();
            }
        }
    }

    void OnArriveAtCenter()
    {
        gameObject.SetActive(false); // hide this bowl cookie
        // TODO: show the shared CookieHalf_Left / CookieHalf_Right here
    }
}