using UnityEngine;

public class CookieTouchInput : MonoBehaviour
{
    [Header("References")]
    public CookieDrag cookieDrag;
    public Transform leftHalf;
    public Transform rightHalf;

    [Header("Touch Areas")]
    [Range(0.1f, 0.9f)]
    public float leftSideLimit = 0.5f;

    [Header("Movement")]
    public float leftMoveLimit = 2f;
    public float rightMoveLimit = 2f;
    public float openThreshold = 1.5f;

    private Camera cam;

    private int leftFingerId = -1;
    private int rightFingerId = -1;

    private Vector3 leftStartPos;
    private Vector3 rightStartPos;

    private bool hasOpened = false;

    void Start()
    {
        cam = Camera.main;

        leftStartPos = leftHalf.localPosition;
        rightStartPos = rightHalf.localPosition;
    }

    void Update()
    {
#if UNITY_EDITOR
        return; // use mouse in editor
#endif

        if (hasOpened || cookieDrag == null || !cookieDrag.isActive)
            return;

        HandleTouches();
    }

    void HandleTouches()
    {
        foreach (Touch touch in Input.touches)
        {
            // LEFT THUMB
            if (touch.phase == TouchPhase.Began &&
                touch.position.x < Screen.width * leftSideLimit &&
                leftFingerId == -1)
            {
                leftFingerId = touch.fingerId;
            }

            // RIGHT THUMB
            if (touch.phase == TouchPhase.Began &&
                touch.position.x >= Screen.width * leftSideLimit &&
                rightFingerId == -1)
            {
                Vector3 world = cam.ScreenToWorldPoint(touch.position);
                world.z = 0;

                Collider2D hit = Physics2D.OverlapPoint(world, cookieDrag.draggableLayer);

                if (hit != null && hit.transform == rightHalf)
                {
                    rightFingerId = touch.fingerId;

                    if (cookieDrag.cookieBreakSFX != null)
                        cookieDrag.cookieBreakSFX.Play();
                }
            }

            // RELEASE
            if (touch.phase == TouchPhase.Ended ||
                touch.phase == TouchPhase.Canceled)
            {
                if (touch.fingerId == leftFingerId)
                    leftFingerId = -1;

                if (touch.fingerId == rightFingerId)
                    rightFingerId = -1;
            }
        }

        MoveLeftHalf();
        MoveRightHalf();

        CheckOpen();
    }

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

    void CheckOpen()
    {
        float gap = rightHalf.localPosition.x - leftHalf.localPosition.x;

        if (gap >= openThreshold)
        {
            hasOpened = true;

            // Trigger your existing open/reveal logic
            cookieDrag.AutoOpen();
        }
    }

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