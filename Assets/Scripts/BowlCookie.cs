using UnityEngine;

//script that manages moving the cookie from the bowl to the center of the screen and activating the fortune system
public class BowlCookie : MonoBehaviour
{
    //the position and scale of the cookie when it is in the center of the screen
    public Vector3 centerPosition = new Vector3(0, 0, 0);
    public Vector3 centerScale = new Vector3(1.5f, 1.5f, 1.5f);
    public float moveSpeed = 5f;

    //checks if the cookie is clickable and if it has been clicked
    //needs to be fixed rn
    public LayerMask clickableCookie;

    //calls the cookiedrag script to activate the fortune system when the cookie has arrived at the center
    public CookieDrag cookieDrag;

    //bool to check if the cookie has been selected and if it has arrived at the center of the screen
    private bool isSelected = false;
    private bool hasArrived = false;

    //so that the player cant click all of the cookies if this is ticked as true
    public static bool cookieAlreadyChosen = false;

    private Camera cam;

    //stores the original position, scale, and rotation
    //so that the cookie can return to the bowl when try again is pressed
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;

    //initializes camera for the layer
    private void Awake()
    {
        cam = Camera.main;

        //stores the original transform values
        originalPosition = transform.position;
        originalScale = transform.localScale;
        originalRotation = transform.rotation;
    }

    void Update()
    {
        //if the player clicks down on their left mouse button
        if (Input.GetMouseButtonDown(0))
        {
            //it will first check if a cookie has already been chosen or not
            //if yes, then return
            //so that the player cant click all of the cookies if this is ticked as true
            if (cookieAlreadyChosen)
                return;

            //checks if the mouse position is over the cookie and if it is, then it will set the cookie as selected
            Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(mousePos, clickableCookie);

            //if the cookie has been clicked, then it will set the cookie as selected and set the cookieAlreadyChosen to true
            if (hit != null)
            {
                cookieAlreadyChosen = true;
                isSelected = true;
            }
        }

        //this is for everything related to moving the cookie
        //to the center of the screen and scaling it up
        if (isSelected && !hasArrived)
        {
            //moves it upwards
            transform.position = Vector3.Lerp(
                transform.position,
                centerPosition,
                moveSpeed * Time.deltaTime
            );

            //then scales it up
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                centerScale,
                moveSpeed * Time.deltaTime
            );

            //clamps its rotation so that it is always facing the camera and not rotating
            //in the case that it had a different rotation due to the random placement in the bowl
            Quaternion targetRotation =
                Quaternion.Euler(0, 0, 0);

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                moveSpeed * Time.deltaTime
            );

            //checks if the cookie has arrived at the center of the screen
            //and if it has, then it will set the hasArrived to true and call the OnArriveAtCenter method
            if (Vector3.Distance(transform.position, centerPosition) < 0.01f)
            {
                hasArrived = true;
                OnArriveAtCenter();
            }
        }
    }

    //if the cookie has arrived at the center then activate the fortune system in cookiedrag and set the isActive flag to true
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

    //resets this cookie back to its original bowl position
    public void ResetCookie()
    {
        //reset the cookie states
        isSelected = false;
        hasArrived = false;

        //reset the cookie transform
        transform.position = originalPosition;
        transform.localScale = originalScale;
        transform.rotation = originalRotation;
    }
}