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

    // Used to figure out which cookie is actually on top when several
    // overlap in the pile, so a click picks the one the player can see
    // and not one buried underneath it. Drag in the child GameObject's
    // Sprite Renderer here (since the script sits on the parent).
    public SpriteRenderer cookieSpriteRenderer;

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

            // Gather EVERY cookie collider under the click point, since
            // cookies in the pile can overlap each other. A single
            // OverlapPoint call isn't enough here - it only returns one
            // collider, and there's no guarantee it's the one that's
            // actually visible on top.
            Collider2D[] hits =
                Physics2D.OverlapPointAll(
                    mousePos,
                    clickableCookie
                );

            if (hits.Length > 0)
            {
                Collider2D topHit = GetTopmostHit(hits);

                // Only select if THIS cookie is the one actually on top.
                // If some other overlapping cookie is more visible at
                // this point, this instance just does nothing - that
                // other cookie's own Update() will pick up the click
                // on this same frame instead.
                if (topHit != null && topHit.transform == transform)
                {
                    cookieAlreadyChosen = true;
                    isSelected = true;
                }
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


    // Out of every collider under the click point, finds whichever one
    // is rendered on top (highest SpriteRenderer.sortingOrder). This is
    // what actually makes pile-picking accurate - sorting order reflects
    // what the player can SEE, not just which collider happened to be
    // first in the physics query results.
    Collider2D GetTopmostHit(Collider2D[] hits)
    {
        Collider2D topHit = null;
        int topOrder = int.MinValue;

        foreach (Collider2D hit in hits)
        {
            // The collider lives on the parent GameObject, same as this
            // script, so read the sprite renderer off that cookie's own
            // BowlCookie component instead of looking on the collider itself.
            BowlCookie bowlCookie = hit.GetComponent<BowlCookie>();

            SpriteRenderer sr =
                (bowlCookie != null)
                ? bowlCookie.cookieSpriteRenderer
                : null;

            // Falls back to 0 if this cookie's Sprite Renderer field
            // hasn't been assigned yet, so a missing setup doesn't crash -
            // just won't be picked accurately until it's wired up.
            int order = (sr != null) ? sr.sortingOrder : 0;

            if (order > topOrder)
            {
                topOrder = order;
                topHit = hit;
            }
        }

        return topHit;
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