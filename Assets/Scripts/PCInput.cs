using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CookieDrag : MonoBehaviour
{
    [Header("General References")]

    //left and right cookie
    public Transform leftHalf;
    public Transform rightHalf;

    //fortune related references
    public Transform fortunePaperTransform;
    public Transform paperMaskTransform;
    public FortuneDatabase fortuneDatabase;
    public TextMeshProUGUI fortuneText;
    public GameObject fortuneTextObject;
    public RectTransform textMaskRect;

    //for the text mask so the text only appears
    //when and while the cookies opening
    [Header("Fortune Text Mask Width")]
    public float closedTextMaskWidth = 0f;
    public float openTextMaskWidth = 300f;

    //sets which layer's draggable
    //draggablelayer is only for the right half of the cookie
    //for pc input
    //openthreshold checks how open the cookie should b
    //as in the gap between left and right cookie
    [Header("Dragging")]
    public LayerMask draggableLayer;
    public float openThreshold = 1.5f;

    //since the paper needs to scale up/extend
    //to make it look like its unfurling from the right cookie
    [Header("Paper")]
    public float closedPaperScaleX = 0.6f;
    public float paperOpenScaleX = 2f;
    public Transform fortunePaperScaleTarget; // the actual paper sprite, not the mask

    [Header("Paper Mask")]
    public float closedPaperMaskScaleX = 0.6f;
    public float paperMaskOpenScaleX = 2f;

    //when the right cookie breaks off
    [Header("Break")]
    public float throwRightDistance = 2f;
    public float dropDistance = 3f;
    public float dropSpeed = 5f;

    //since i wanted the cookie to center itself
    //cuz it looks awkward if left stationary in the middle
    [Header("Center Adjustment")]
    public float leftShiftAmount = 0.35f;
    public float dragLeftShiftAmount = 0.15f;
    public float shiftSpeed = 5f;

    //drop tilt for the cookie
    //can b removed
    [Header("Drop Tilt")]
    public float dropTiltAngle = -25f;
    public float dropTiltSpeed = 5f;

    //reveal canvas for the try again part
    [Header("Reveal Canvas")]
    public CanvasGroup revealOverlay; // CanvasGroup on the reveal canvas, used to fade it in
    public TextMeshProUGUI revealFortuneText; // big fortune text shown in the middle of the reveal canvas
    public float delayBeforeOverlay = 2f;
    public float fadeSpeed = 2f;

    //auto open, triggered by a button instead of dragging
    [Header("Auto Open")]
    public float autoOpenSpeed = 2f; // units per second

    [HideInInspector]
    public bool isActive = false;

    //bool for right cookie and reveal overlay
    private bool isDragging;
    private bool isOpened;
    private bool isDropping;
    private bool isWaiting;
    private bool isFading;
    private bool isShifting;
    private bool isAutoOpening;

    //wait timer for the reveal overlay
    private float waitTimer;
    private float dragDistance;

    private Camera cam;

    private Vector3 leftDragStartLocalPos;
    private Vector3 rightBaseLocalPos;
    private Quaternion rightBaseRot;

    private Vector3 leftTargetPos;
    private Vector3 paperTargetPos;
    private Vector3 dropTarget;

    void Start()
    {
        cam = Camera.main;

        leftDragStartLocalPos = leftHalf.localPosition;
        rightBaseLocalPos = rightHalf.localPosition;
        rightBaseRot = rightHalf.localRotation;

        leftTargetPos = leftHalf.position;
        paperTargetPos = fortunePaperTransform.position;

        revealOverlay.alpha = 0f;
        revealOverlay.interactable = false;
        revealOverlay.blocksRaycasts = false;

        if (fortuneTextObject != null)
        {
            fortuneTextObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!isActive) return;

        if (isShifting)
        {
            HandleShifting();
            return;
        }

        if (isDropping)
        {
            HandleDropping();
            return;
        }

        if (isWaiting)
        {
            HandleWaiting();
            return;
        }

        if (isFading)
        {
            HandleFading();
            return;
        }

        if (isOpened) return;

        if (isAutoOpening)
        {
            HandleAutoOpen();
            return;
        }

        HandleDrag();
    }

    bool MoveToward(Transform t, Vector3 target, float speed)
    {
        t.position = Vector3.MoveTowards(t.position, target, speed * Time.deltaTime);
        return Vector3.Distance(t.position, target) < 0.01f;
    }

    void HandleShifting()
    {
        bool leftDone = MoveToward(leftHalf, leftTargetPos, shiftSpeed);
        fortunePaperTransform.position = Vector3.MoveTowards(fortunePaperTransform.position, paperTargetPos, shiftSpeed * Time.deltaTime);

        if (leftDone) isShifting = false;
    }

    void HandleDropping()
    {
        bool rightDone = MoveToward(rightHalf, dropTarget, dropSpeed);
        rightHalf.rotation = Quaternion.Lerp(rightHalf.rotation, rightBaseRot * Quaternion.Euler(0, 0, dropTiltAngle), dropTiltSpeed * Time.deltaTime);

        if (rightDone)
        {
            isDropping = false;
            isWaiting = true;
            waitTimer = 0f;
        }
    }

    void HandleWaiting()
    {
        waitTimer += Time.deltaTime;
        if (waitTimer >= delayBeforeOverlay)
        {
            isWaiting = false;
            isFading = true;

            if (revealFortuneText != null && fortuneText != null)
            {
                revealFortuneText.text = fortuneText.text;
            }
        }
    }

    void HandleFading()
    {
        revealOverlay.alpha = Mathf.MoveTowards(revealOverlay.alpha, 1f, fadeSpeed * Time.deltaTime);
    }

    public void ActivateFortune()
    {
        if (fortuneText != null && fortuneDatabase != null)
        {
            fortuneText.text = fortuneDatabase.GetRandomFortune();
            fortuneText.maxVisibleCharacters = fortuneText.text.Length;
        }

        SetMasks(closedTextMaskWidth, closedPaperScaleX, closedPaperMaskScaleX);

        if (fortuneTextObject != null)
        {
            fortuneTextObject.SetActive(true);
        }
    }

    //call this from a UI Button's OnClick() to auto-open the cookie
    //without needing the player to drag
    public void AutoOpen()
    {
        if (isOpened || isAutoOpening) return;
        isDragging = false; // cancel any manual drag in progress
        isAutoOpening = true;
    }

    //call this from the "Try Again" button's OnClick() on the reveal canvas
    //reloads the current scene, resetting everything
    public void TryAgain()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    void HandleDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(mousePos, draggableLayer);

            if (hit != null && hit.transform == rightHalf)
            {
                isDragging = true;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (!isDragging) return;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;

        Vector3 mouseLocal = rightHalf.parent.InverseTransformPoint(mouseWorld);
        float dragAmount = Mathf.Max(mouseLocal.x - rightBaseLocalPos.x, 0);

        rightHalf.localPosition = new Vector3(rightBaseLocalPos.x + dragAmount, rightBaseLocalPos.y, rightBaseLocalPos.z);
        rightHalf.localRotation = rightBaseRot;

        dragDistance = dragAmount;

        float dragT = Mathf.InverseLerp(0, openThreshold, dragDistance);
        leftHalf.localPosition = Vector3.Lerp(leftDragStartLocalPos, leftDragStartLocalPos + Vector3.left * dragLeftShiftAmount, dragT);

        UpdatePaper(dragT);
        CheckOpen();
    }

    //mirrors HandleDrag's math, but driven by time instead of mouse position
    //so the auto-open animates exactly like a real drag would
    void HandleAutoOpen()
    {
        dragDistance += autoOpenSpeed * Time.deltaTime;
        dragDistance = Mathf.Min(dragDistance, openThreshold);

        rightHalf.localPosition = new Vector3(rightBaseLocalPos.x + dragDistance, rightBaseLocalPos.y, rightBaseLocalPos.z);
        rightHalf.localRotation = rightBaseRot;

        float dragT = Mathf.InverseLerp(0, openThreshold, dragDistance);
        leftHalf.localPosition = Vector3.Lerp(leftDragStartLocalPos, leftDragStartLocalPos + Vector3.left * dragLeftShiftAmount, dragT);

        UpdatePaper(dragT);

        if (dragDistance >= openThreshold)
        {
            isAutoOpening = false;
            CheckOpen();
        }
    }

    void UpdatePaper(float t)
    {
        float textWidth = Mathf.Lerp(closedTextMaskWidth, openTextMaskWidth, t);
        float paperScale = Mathf.Lerp(closedPaperScaleX, paperOpenScaleX, t);
        float paperMaskScale = Mathf.Lerp(closedPaperMaskScaleX, paperMaskOpenScaleX, t);
        SetMasks(textWidth, paperScale, paperMaskScale);
    }

    void SetMasks(float textWidth, float paperScaleX, float paperMaskScaleX)
    {
        Vector3 maskScale = paperMaskTransform.localScale;
        maskScale.x = paperMaskScaleX;
        paperMaskTransform.localScale = maskScale;

        if (fortunePaperScaleTarget != null)
        {
            Vector3 paperScale = fortunePaperScaleTarget.localScale;
            paperScale.x = paperScaleX;
            fortunePaperScaleTarget.localScale = paperScale;
        }

        if (textMaskRect != null)
        {
            Vector2 size = textMaskRect.sizeDelta;
            size.x = textWidth;
            textMaskRect.sizeDelta = size;
        }
    }


    //checks the open threshold
    void CheckOpen()
    {
        if (dragDistance < openThreshold) return;

        isOpened = true;
        isDragging = false;

        SetMasks(openTextMaskWidth, paperOpenScaleX, paperMaskOpenScaleX);

        leftTargetPos = leftHalf.position + Vector3.left * leftShiftAmount;
        paperTargetPos = fortunePaperTransform.position + Vector3.left * leftShiftAmount;
        isShifting = true;

        Vector3 currentRightPos = rightHalf.position;
        rightHalf.SetParent(null, true);
        rightHalf.position = currentRightPos;

        dropTarget = currentRightPos + new Vector3(throwRightDistance, -dropDistance, 0);
        isDropping = true;
    }
}