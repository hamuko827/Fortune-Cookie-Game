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

    //mask where the paper is visible inside of
    //same value as paper so if the player moves the right cookie
    //the mask moves alongside it
    //and the paper becomes visible
    [Header("Paper Mask")]
    public float closedPaperMaskScaleX = 0.6f;
    public float paperMaskOpenScaleX = 2f;

    //effects for when the right cookie breaks off
    [Header("Break")]
    public float throwRightDistance = 2f;
    public float dropDistance = 3f;
    public float dropSpeed = 5f;

    //particle system
    public ParticleSystem crumbParticles;

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
    //canvas group for the fade in effect using alpha
    public CanvasGroup revealOverlay;
    public TextMeshProUGUI revealFortuneText;
    public float delayBeforeOverlay = 2f;
    public float fadeSpeed = 2f;

    //auto open, triggered by a button instead of dragging
    [Header("Auto Open")]
    public float autoOpenSpeed = 2f; // units per second

    //all sfx related references
    [Header("SFX")]
    public AudioSource cookieBreakSFX;


    //everything below this are private variables
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

    //sets all variables at the start
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

        cookieBreakSFX = GetComponent<AudioSource>();

        if (fortuneTextObject != null)
        {
            fortuneTextObject.SetActive(false);
        }
    }

    //everything that happens every frame so it checks whats currently happening to each cookie
    //and handling the method corresponding to that state
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

        //always handling drag unless the cookie is opened or auto opening, so that the player can drag the right cookie half
        HandleDrag();
    }

    //moves the transform t towards the target position at a given speed
    bool MoveToward(Transform t, Vector3 target, float speed)
    {
        t.position = Vector3.MoveTowards(t.position, target, speed * Time.deltaTime);
        return Vector3.Distance(t.position, target) < 0.01f;
    }

    //spawns the crumbs particle system at the given position
    //to show off the effect of crumbs
    public void SpawnCrumbs(Vector2 position)
    {
        if (crumbParticles == null) return;
        crumbParticles.transform.position = position;
        crumbParticles.Play();
    }

    //this is for the slight shifting to the left that i added
    //to ensure that the cookie is always centered when the player opens it, since the right cookie half is moving to the right
    void HandleShifting()
    {
        bool leftDone = MoveToward(leftHalf, leftTargetPos, shiftSpeed);
        fortunePaperTransform.position = Vector3.MoveTowards(fortunePaperTransform.position, paperTargetPos, shiftSpeed * Time.deltaTime);

        if (leftDone) isShifting = false;
    }
    
    //manages the dropping of the right cookie
    void HandleDropping()
    {
        //if bool rightDone (means right cookie half has fallen)
        //then activate the dropping part
        bool rightDone = MoveToward(rightHalf, dropTarget, dropSpeed);
        rightHalf.rotation = Quaternion.Lerp(rightHalf.rotation, rightBaseRot * Quaternion.Euler(0, 0, dropTiltAngle), dropTiltSpeed * Time.deltaTime);

        if (rightDone)
        {
            isDropping = false;
            isWaiting = true;
            waitTimer = 0f;
        }
    }
    
    //this is for the delay before showing the reveal screen
    //checks first if there is already a revealed fortune text 
    //and if fortune text is not null
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

    //in charge of the fading of the reveal overlay depending on the fadespeed i put in inspector
    void HandleFading()
    {
        revealOverlay.alpha = Mathf.MoveTowards(revealOverlay.alpha, 1f, fadeSpeed * Time.deltaTime);
    }

    //calls the fortune database to actually generate a fortune for the player
    public void ActivateFortune()
    {
        //if the fortune text and database are not null
        //then proceed with generating a fortune and setting the text to the fortune text
        if (fortuneText != null && fortuneDatabase != null)
        {
            fortuneText.text = fortuneDatabase.GetRandomFortune();
            fortuneText.maxVisibleCharacters = fortuneText.text.Length;
        }

        //for the mask, sets the mask to the closed state so that the text is hidden until the player opens the cookie
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
        isDragging = false; 
        isAutoOpening = true;
    }

    //call this from the "Try Again" button's OnClick() on the reveal canvas
    //reloads the current scene, resetting everything
    public void TryAgain()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void ForceOpen()
    {
        if (isOpened) return;

        dragDistance = openThreshold;
        CheckOpen();
    }

    //everything related to dragging the right cookie half, checking if the player is clicking on it and moving it to the right
    void HandleDrag()
    {

        //checks the mouse and translates the mouse position to an actual world point
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(mousePos, draggableLayer);

            //if the player clicks on the right cookie half, then it will spawn crumbs and set isdragging as true
            if (hit != null && hit.transform == rightHalf)
            {
                SpawnCrumbs(transform.position + Vector3.down * 0.2f);
                isDragging = true;
                cookieBreakSFX.Play();
            }
        }

        //sets isdragging as false if the player lifts their click from the left mouse button
        //since it is get mouse button up
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
    //i have not set a time for this yet
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

    //updates the paper scale for the masks
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
        //if the drag distance is less than the open threshold, then return and do nothing
        //cuz that means the player has not dragged the right cookie half far enough to open it
        if (dragDistance < openThreshold) return;

        //if it is not less than the open threshold, then it will set isopened as true and isdragging as false
        //cuz that means the gap between the two cookie halves is big enough to trigger the opening of the cookie
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