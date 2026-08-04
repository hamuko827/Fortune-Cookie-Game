using UnityEngine;

public class BowlCookie : MonoBehaviour
{
    public Vector3 centerPosition = new Vector3(0, 0, 0);
    public Vector3 centerScale = new Vector3(1.5f, 1.5f, 1.5f);
    public float moveSpeed = 5f;

    public GameObject cookieHalfLeft;
    public GameObject cookieHalfRight;
    public CookieDrag cookieDrag; // NEW: direct reference, drag in Inspector

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

            if (Vector3.Distance(transform.position, centerPosition) < 0.01f)
            {
                hasArrived = true;
                OnArriveAtCenter();
            }
        }
    }

    void OnArriveAtCenter()
    {
        gameObject.SetActive(false);
        cookieHalfLeft.SetActive(true);
        cookieHalfRight.SetActive(true);

        if (cookieDrag != null)
        {
            cookieDrag.isActive = true; // NEW: only now allow dragging
        }
    }
}