using UnityEngine;

public class BowlCookie : MonoBehaviour
{
    public Vector3 centerPosition = new Vector3(0, 0, 0);
    public Vector3 centerScale = new Vector3(1.5f, 1.5f, 1.5f);
    public float moveSpeed = 5f;

    public LayerMask clickableCookie;

    public CookieDrag cookieDrag;

    private bool isSelected = false;
    private bool hasArrived = false;

    public static bool cookieAlreadyChosen = false;

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (cookieAlreadyChosen)
                return;

            Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(mousePos, clickableCookie);

            if (hit != null)
            {
                cookieAlreadyChosen = true;
                isSelected = true;
            }
        }

        if (isSelected && !hasArrived)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                centerPosition,
                moveSpeed * Time.deltaTime
            );


            transform.localScale = Vector3.Lerp(
                transform.localScale,
                centerScale,
                moveSpeed * Time.deltaTime
            );

            Quaternion targetRotation =
                Quaternion.Euler(0, 0, 0);

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, centerPosition) < 0.01f)
            {
                hasArrived = true;
                OnArriveAtCenter();
            }
        }
    }

    void OnArriveAtCenter()
    {
        if (cookieDrag != null)
        {
            //activate the fortune method in cookiedrag
            cookieDrag.ActivateFortune();

            //set cookiedrag is active flag
            cookieDrag.isActive = true;
        }
    }
}
    