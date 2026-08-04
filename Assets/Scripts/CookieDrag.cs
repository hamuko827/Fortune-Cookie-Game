using UnityEngine;
public class CookieDrag : MonoBehaviour
{
    public Transform leftHalf;
    public Transform rightHalf;
    public GameObject fortunePaper;
    public float openThreshold = 1.5f;
    public float maxTiltAngle = 15f;
    public CanvasGroup revealOverlay;
    public float fadeSpeed = 2f;
    public float delayBeforeOverlay = 2f;
    private bool isOpened = false;
    private bool isFading = false;
    private bool isWaiting = false;
    private float waitTimer = 0f;
    public bool isActive = false;
    private float closedGap;
    private Quaternion leftBaseRotation;
    private Quaternion rightBaseRotation;
    void Start()
    {
        closedGap = Vector3.Distance(leftHalf.position, rightHalf.position);
        leftBaseRotation = leftHalf.rotation;
        rightBaseRotation = rightHalf.rotation;
    }
    void Update()
    {
        if (!isActive) return;
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= delayBeforeOverlay)
            {
                isWaiting = false;
                isFading = true;
            }
            return;
        }
        if (isFading)
        {
            revealOverlay.alpha = Mathf.MoveTowards(revealOverlay.alpha, 1f, fadeSpeed * Time.deltaTime);
            if (revealOverlay.alpha >= 1f)
            {
                isFading = false;
            }
            return;
        }
        if (isOpened) return;
#if UNITY_ANDROID || UNITY_IOS
        HandleMobileInput();
#else
        HandlePCInput();
#endif
    }
void HandlePCInput()
{
    if (Input.GetMouseButton(0))
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        float dragOffset = (mouseWorldPos.x - rightHalf.position.x) * 0.5f;

        Vector3 newRightPos = rightHalf.position + new Vector3(dragOffset, 0, 0);
        Vector3 newLeftPos = leftHalf.position - new Vector3(dragOffset, 0, 0);

        // NEW: only allow the move if it doesn't shrink the gap below the closed distance
        float newGap = Vector3.Distance(newLeftPos, newRightPos);
        if (newGap >= closedGap)
        {
            rightHalf.position = newRightPos;
            leftHalf.position = newLeftPos;
        }

        ApplyTilt();
        CheckOpenDistance();
    }
}

void HandleMobileInput()
{
    if (Input.touchCount >= 2)
    {
        Touch touch1 = Input.GetTouch(0);
        Touch touch2 = Input.GetTouch(1);

        Vector3 touch1WorldPos = Camera.main.ScreenToWorldPoint(touch1.position);
        Vector3 touch2WorldPos = Camera.main.ScreenToWorldPoint(touch2.position);
        touch1WorldPos.z = 0;
        touch2WorldPos.z = 0;

        Vector3 newLeftPos = new Vector3(touch1WorldPos.x, leftHalf.position.y, 0);
        Vector3 newRightPos = new Vector3(touch2WorldPos.x, rightHalf.position.y, 0);

        // NEW: same clamp check for mobile
        float newGap = Vector3.Distance(newLeftPos, newRightPos);
        if (newGap >= closedGap)
        {
            leftHalf.position = newLeftPos;
            rightHalf.position = newRightPos;
        }

        ApplyTilt();
        CheckOpenDistance();
    }
}
    void ApplyTilt()
    {
        float currentGap = Vector3.Distance(leftHalf.position, rightHalf.position);
        float openFraction = Mathf.Clamp01((currentGap - closedGap) / (openThreshold - closedGap));
        leftHalf.rotation = leftBaseRotation * Quaternion.Euler(0, 0, openFraction * maxTiltAngle);
        rightHalf.rotation = rightBaseRotation * Quaternion.Euler(0, 0, -openFraction * maxTiltAngle);
    }
    void CheckOpenDistance()
    {
        float distance = Vector3.Distance(leftHalf.position, rightHalf.position);
        if (distance >= openThreshold)
        {
            isOpened = true;
            isWaiting = true;
            waitTimer = 0f;
            Debug.Log("Fortune revealed, waiting to show overlay...");
        }
    }
}