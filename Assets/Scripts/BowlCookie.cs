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

// Keeps track of which cookie is currently in the center.
private static BowlCookie currentSelectedCookie;

private Camera cam;

// Used to figure out which cookie is actually on top when several
// overlap in the pile, so a click picks the one the player can see
// and not one buried underneath it.
private SpriteRenderer spriteRenderer;

// Stores the original position, scale, and rotation
// so that the cookie can return to the bowl when Try Again is pressed.
private Vector3 originalPosition;
private Vector3 originalScale;
private Quaternion originalRotation;

//sound effect when the cookie is selected and starts scaling up
[Header("SFX")]
public AudioSource cookieSelectSFX;


// Initializes camera and stores the original transform values.
private void Awake()
{
    cam = Camera.main;

    spriteRenderer = GetComponent<SpriteRenderer>();

    originalPosition = transform.position;
    originalScale = transform.localScale;
    originalRotation = transform.rotation;
}


void Update()
{
    // If the player clicks down on their left mouse button.
    if (Input.GetMouseButtonDown(0))
    {
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
                // If this cookie is already selected,
                // don't do anything.
                if (currentSelectedCookie == this)
                    return;

                // Remember the old cookie before switching.
                BowlCookie oldCookie =
                    currentSelectedCookie;

                // Make the new cookie the current cookie.
                currentSelectedCookie = this;
                cookieAlreadyChosen = true;

                // Reset the old cookie completely.
                if (oldCookie != null &&
                    oldCookie != this)
                {
                    oldCookie.ResetCookie();
                }

                // This cookie is now selected.
                isSelected = true;
                hasArrived = false;

                // Play the selection sound once when
                // the cookie starts scaling up.
                if (cookieSelectSFX != null)
                {
                    cookieSelectSFX.Play();
                }
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
        SpriteRenderer sr =
            hit.GetComponent<SpriteRenderer>();

        // Falls back to 0 if this collider's object doesn't have
        // its own SpriteRenderer (shouldn't normally happen for
        // a cookie, but avoids a crash if the layer mask ever
        // catches something unexpected).
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


// Lets CookieDrag check whether THIS is the cookie
// currently selected by the player.
public bool IsCurrentSelectedCookie()
{
    return currentSelectedCookie == this;
}


// Resets this cookie back to its original bowl position.
public void ResetCookie()
{
    // Stop CookieDrag FIRST so it cannot process
    // any more input while this cookie is being reset.
    if (cookieDrag != null)
    {
        cookieDrag.isActive = false;
    }

    // IMPORTANT:
    //
    // The MAIN COOKIE must return to its original position FIRST.
    //
    // CookieDrag.ResetCookie() restores the fortune paper using
    // world-space positions. Since the fortune paper is part of
    // this cookie's hierarchy, restoring the main cookie AFTER
    // the paper would move the paper a second time and place it
    // somewhere incorrect.
    transform.position = originalPosition;
    transform.localScale = originalScale;
    transform.rotation = originalRotation;

    // NOW reset CookieDrag.
    //
    // At this point the main cookie is already back in the bowl,
    // so the paper's original world position is restored relative
    // to the correct parent position.
    if (cookieDrag != null)
    {
        cookieDrag.ResetCookie();
    }

    // Reset the cookie states.
    isSelected = false;
    hasArrived = false;
}


// Completely clears the current cookie selection.
// Used when the game is restarted.
public static void ClearCurrentSelection()
{
    currentSelectedCookie = null;
    cookieAlreadyChosen = false;
}
}