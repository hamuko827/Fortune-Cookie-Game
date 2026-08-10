using UnityEngine;

// Script that manages moving the cookie from the bowl to the center
// of the screen and activating the fortune system.
public class BowlCookie : MonoBehaviour
{
    // The position and scale of the cookie when it is in the center of the screen.
    public Vector3 centerPosition = new Vector3(0, 0, 0);
    public Vector3 centerScale = new Vector3(1.5f, 1.5f, 1.5f);
    public float moveSpeed = 5f;

    // Checks if the cookie is clickable and if it has been clicked.
    public LayerMask clickableCookie;

    // Calls the CookieDrag script to activate the fortune system
    // when the cookie has arrived at the center.
    public CookieDrag cookieDrag;

    // Bool to check if the cookie has been selected
    // and if it has arrived at the center of the screen.
    private bool isSelected = false;
    private bool hasArrived = false;

    // So that the player can't click all of the cookies.
    public static bool cookieAlreadyChosen = false;

    private Camera cam;

    // Stores the original position, scale, and rotation
    // so that the cookie can return to the bowl when Try Again is pressed.
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;


    // Initializes camera and stores the original transform values.
    private void Awake()
    {
        cam = Camera.main;

        originalPosition = transform.position;
        originalScale = transform.localScale;
        originalRotation = transform.rotation;
    }


    void Update()
    {
        // If the player clicks down on their left mouse button.
        if (Input.GetMouseButtonDown(0))
        {
            // If a cookie has already been chosen, don't allow another one.
            if (cookieAlreadyChosen)
                return;

            // Check the mouse position.
            Vector2 mousePos =
                cam.ScreenToWorldPoint(Input.mousePosition);

            Collider2D hit =
                Physics2D.OverlapPoint(
                    mousePos,
                    clickableCookie
                );

            // If the cookie has been clicked,
            // select it and prevent other cookies from being selected.
            if (hit != null)
            {
                cookieAlreadyChosen = true;
                isSelected = true;
            }
        }


        // Move the cookie to the center of the screen.
        if (isSelected && !hasArrived)
        {
            // Move upwards/to the center.
            transform.position = Vector3.Lerp(
                transform.position,
                centerPosition,
                moveSpeed * Time.deltaTime
            );

            // Scale the cookie up.
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                centerScale,
                moveSpeed * Time.deltaTime
            );

            // Reset rotation so the cookie is facing the camera.
            Quaternion targetRotation =
                Quaternion.Euler(0, 0, 0);

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                moveSpeed * Time.deltaTime
            );


            // Check if the cookie has arrived at the center.
            if (Vector3.Distance(
                transform.position,
                centerPosition
            ) < 0.01f)
            {
                hasArrived = true;
                OnArriveAtCenter();
            }
        }
    }


    // Called once the cookie has arrived at the center.
    void OnArriveAtCenter()
    {
        // Activate the fortune system in CookieDrag.
        if (cookieDrag != null)
        {
            cookieDrag.ActivateFortune();

            // Allow CookieDrag to receive input.
            cookieDrag.isActive = true;
        }

        // IMPORTANT:
        // We do NOT enable/disable the highlight manager here.
        //
        // CookieHighlightManager now watches all BowlCookie objects
        // and automatically determines which ones are at the center.
    }


    // Lets CookieHighlightManager check whether this cookie
    // has arrived at the center.
    public bool HasArrived()
    {
        return hasArrived;
    }


    // Resets this cookie back to its original bowl position.
    public void ResetCookie()
    {
        // Reset the cookie states.
        isSelected = false;
        hasArrived = false;

        // Reset the cookie transform.
        transform.position = originalPosition;
        transform.localScale = originalScale;
        transform.rotation = originalRotation;
    }
}
