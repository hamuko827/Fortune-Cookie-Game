using UnityEngine;

//everything related to touch input for mobile
public class CookieTouchInput : MonoBehaviour
{
    //general references that searches for the left and right half
    [Header("References")]
    public CookieDrag cookieDrag;
    public Transform leftHalf;
    public Transform rightHalf;

    //the areas that can be touched, starting from 0.1f to 0.9f
    [Header("Touch Areas")]
    [Range(0.1f, 0.9f)]
    public float leftSideLimit = 0.5f;

    //the limits for left and right to be moved
    //and the actual openthreshold
    [Header("Movement")]
    public float leftMoveLimit = 2f;
    public float rightMoveLimit = 2f;
    public float openThreshold = 1.5f;

    //initializes camera for the layer to see which cookie part is draggable
    private Camera cam;

    //finger ids
    private int leftFingerId = -1;
    private int rightFingerId = -1;
    
    //starting position of both sides
    private Vector3 leftStartPos;
    private Vector3 rightStartPos;

    //bool to check if the cookie has been opened or if openthreshold has been reached
    //to activate the fortune system just like in the pc input
    private bool hasOpened = false;

    //initializes camera and starting positions of both sides
    void Start()
    {
        cam = Camera.main;

        leftStartPos = leftHalf.localPosition;
        rightStartPos = rightHalf.localPosition;
    }

    void Update()
    {

#if UNITY_EDITOR
        return;
#endif
        //currently unreachable code, but if the cookie has been opened or if the cookie drag is
        //null or not active, it will return and not handle touches
        if (hasOpened || cookieDrag == null || !cookieDrag.isActive)
            return;

        HandleTouches();
    }

    void HandleTouches()
    {
        //checks all touches from the player
        foreach (Touch touch in Input.touches)
        {
            //checks if touch phase has began and if the touch position is on the left side of the screen
            //because if it is, then that counts as the left finger id
            if (touch.phase == TouchPhase.Began &&
                touch.position.x < Screen.width * leftSideLimit &&
                leftFingerId == -1)
            {
                leftFingerId = touch.fingerId;
            }

            //also the same but for the right side = right finger input
            if (touch.phase == TouchPhase.Began &&
                touch.position.x >= Screen.width * leftSideLimit &&
                rightFingerId == -1)
            {
                //converts area touched in screen to world point
                Vector3 world = cam.ScreenToWorldPoint(touch.position);
                world.z = 0;

                //checks draggable layer
                //maybe edit this because right now
                //pc input needs to only have the right side in the draggablelayer
                Collider2D hit = Physics2D.OverlapPoint(world, cookieDrag.draggableLayer);

                //if something has been hit and its the right side
                //then right gingerid will be the touch finger id
                //then play cookiedrag sfx
                if (hit != null && hit.transform == rightHalf)
                {
                    rightFingerId = touch.fingerId;

                    if (cookieDrag.cookieBreakSFX != null)
                        cookieDrag.cookieBreakSFX.Play();
                }
            }

            //for when the player releases the screen or is not touching the screen
            if (touch.phase == TouchPhase.Ended ||
                touch.phase == TouchPhase.Canceled)
            {
                if (touch.fingerId == leftFingerId)
                    leftFingerId = -1;

                if (touch.fingerId == rightFingerId)
                    rightFingerId = -1;
            }
        }

        //calls the methods
        MoveLeftHalf();
        MoveRightHalf();

        CheckOpen();
    }
    
    //moves left half but checks left finger id first
    //if -1 that means no touch phase on the left
    void MoveLeftHalf()
    {
        if (leftFingerId == -1) return;

        Touch touch = GetTouch(leftFingerId);
        if (touch.fingerId == -1) return;

        Vector3 world = cam.ScreenToWorldPoint(touch.position);
        world.z = 0;

        Vector3 local = leftHalf.parent.InverseTransformPoint(world);

        float offset = Mathf.Clamp(local.x - leftStartPos.x, -leftMoveLimit, 0f);

        leftHalf.localPosition = new Vector3(
            leftStartPos.x + offset,
            leftStartPos.y,
            leftStartPos.z);
    }

    //the same but for the right half, but checks right finger id first
    void MoveRightHalf()
    {
        if (rightFingerId == -1) return;

        Touch touch = GetTouch(rightFingerId);
        if (touch.fingerId == -1) return;

        Vector3 world = cam.ScreenToWorldPoint(touch.position);
        world.z = 0;

        Vector3 local = rightHalf.parent.InverseTransformPoint(world);

        float offset = Mathf.Clamp(local.x - rightStartPos.x, 0f, rightMoveLimit);

        rightHalf.localPosition = new Vector3(
            rightStartPos.x + offset,
            rightStartPos.y,
            rightStartPos.z);
    }

    //checks the open threshold to see if its opened or not, if it is then it will trigger the cookiedrag autoopen method
    void CheckOpen()
    {
        float gap = rightHalf.localPosition.x - leftHalf.localPosition.x;

        if (gap >= openThreshold)
        {
            hasOpened = true;

            //triggers the autoopen method in cookiedrag to activate the fortune system
            cookieDrag.AutoOpen();
        }
    }

    //sets the touch to -1 if the finger id is not found, otherwise it will return the touch
    Touch GetTouch(int fingerId)
    {
        foreach (Touch touch in Input.touches)
        {
            if (touch.fingerId == fingerId)
                return touch;
        }

        return new Touch { fingerId = -1 };
    }
}