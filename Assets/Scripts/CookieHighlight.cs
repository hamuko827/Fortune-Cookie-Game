using UnityEngine;

// Manages hover highlights for the LEFT and RIGHT halves
// of every cookie.
//
// The highlight only appears when:
// 1. That cookie has arrived at the center.
// 2. The player is hovering over that specific cookie half.
//
// Each cookie can have:
// - A left cookie half
// - A left highlight
// - A right cookie half
// - A right highlight
public class CookieHighlightManager : MonoBehaviour
{
    [System.Serializable]
    public class CookieHighlight
    {
        [Header("Cookie")]
        public BowlCookie cookie;

        [Header("Left Half")]
        public Transform leftHalf;
        public GameObject leftHighlight;

        [Header("Right Half")]
        public Transform rightHalf;
        public GameObject rightHighlight;
    }

    [Header("Cookies")]
    public CookieHighlight[] cookies;

    private Camera cam;

    // Once the player clicks either cookie half,
    // the hover highlight stays disabled until the cookie is reset.
    private bool highlightDisabled = false;


    void Awake()
    {
        cam = Camera.main;

        HideAllHighlights();
    }


    void Update()
    {
        if (cam == null)
            return;

        // If the highlight was turned off because the player
        // clicked a cookie half, don't show it again.
        if (highlightDisabled)
        {
            HideAllHighlights();
            return;
        }

        if (cookies == null || cookies.Length == 0)
            return;

        Vector2 mousePosition =
            cam.ScreenToWorldPoint(Input.mousePosition);

        bool clickedCookieHalf = false;


        foreach (CookieHighlight entry in cookies)
        {
            if (entry == null ||
                entry.cookie == null)
            {
                continue;
            }

            // IMPORTANT:
            // The cookie must have reached the CENTER first.
            //
            // cookieAlreadyChosen is NOT checked here because
            // it becomes true while the cookie is travelling
            // from the bowl to the center.
            if (!entry.cookie.HasArrived())
            {
                HideCookieHighlights(entry);
                continue;
            }


            // Check if the mouse is over the LEFT half.
            bool hoveringLeft =
                IsMouseOver(
                    mousePosition,
                    entry.leftHalf
                );


            // Check if the mouse is over the RIGHT half.
            bool hoveringRight =
                IsMouseOver(
                    mousePosition,
                    entry.rightHalf
                );


            // Show ONLY the left highlight when
            // the mouse is over the left cookie.
            if (entry.leftHighlight != null)
            {
                entry.leftHighlight.SetActive(
                    hoveringLeft
                );
            }


            // Show ONLY the right highlight when
            // the mouse is over the right cookie.
            if (entry.rightHighlight != null)
            {
                entry.rightHighlight.SetActive(
                    hoveringRight
                );
            }


            // If the player clicks either half,
            // disable the hover highlight.
            if (Input.GetMouseButtonDown(0) &&
                (hoveringLeft || hoveringRight))
            {
                clickedCookieHalf = true;
            }
        }


        // Turn off the highlight after clicking
        // either cookie half.
        if (clickedCookieHalf)
        {
            highlightDisabled = true;
            HideAllHighlights();
        }
    }


    // Checks whether the mouse is currently over
    // a specific cookie half.
    bool IsMouseOver(
        Vector2 mousePosition,
        Transform cookieHalf
    )
    {
        if (cookieHalf == null)
            return false;

        Collider2D collider =
            cookieHalf.GetComponent<Collider2D>();

        if (collider == null)
            return false;

        return collider.OverlapPoint(mousePosition);
    }


    // Hides both highlights belonging to one cookie.
    void HideCookieHighlights(
        CookieHighlight entry
    )
    {
        if (entry == null)
            return;

        if (entry.leftHighlight != null)
        {
            entry.leftHighlight.SetActive(false);
        }

        if (entry.rightHighlight != null)
        {
            entry.rightHighlight.SetActive(false);
        }
    }


    // Hides every highlight from every cookie.
    void HideAllHighlights()
    {
        if (cookies == null)
            return;

        foreach (CookieHighlight entry in cookies)
        {
            if (entry == null)
                continue;

            HideCookieHighlights(entry);
        }
    }


    // Call this when the cookie is reset / Try Again is pressed.
    public void ResetHighlights()
    {
        highlightDisabled = false;
        HideAllHighlights();
    }
}
