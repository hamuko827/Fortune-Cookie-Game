using UnityEngine;
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
    //NOTE: both leftHalf and rightHalf's colliders now need to be on this layer,
    //since either one can be grabbed
    [Header("Dragging")]
    public LayerMask draggableLayer;
    public float openThreshold = 1.5f;

    //since the paper needs to scale up/extend
    //to make it look like its unfurling from the right cookie
    [Header("Paper")]
    public float closedPaperScaleX = 0.6f;
    public float paperOpenScaleX = 2f;
    public Transform fortunePaperScaleTarget; // actual paper sprite, not the mask

    //mask where the paper is visible inside of
    [Header("Paper Mask")]
    public float closedPaperMaskScaleX = 0.6f;
    public float paperMaskOpenScaleX = 2f;

    //adjustment for the paper mask when the LEFT cookie is dragged
    //use a negative value to move the paper/mask slightly to the left
    //if it is clipping too far to the right
    [Header("Left Drag Paper Adjustment")]
    public float leftDragPaperOffsetX = 0f;

    //ADDITIONAL offset applied progressively as the paper opens (scales
    //from 0 at the start of the drag up to this full value once fully open).
    //use this if leftDragPaperOffsetX alone isn't enough - the paper's
    //scale grows as it opens, so how far it clips outward grows too, and a
    //single fixed offset can't correct for both a mostly-closed paper and
    //a fully-open one at the same time
    public float leftDragPaperOpenOffsetX = 0f;

    //manual anchored position (Pos X / Pos Y on the RectTransform) for the fortune
    //text mask GameObject, since the correct spot differs depending on which cookie
    //half breaks off
    [Header("Fortune Text Mask Placement")]
    public Vector2 textMaskPosRight = Vector2.zero;
    public Vector2 textMaskPosLeft = Vector2.zero;

    //effects for when the cookie half breaks off
    [Header("Break")]
    public float throwRightDistance = 2f;
    public float dropDistance = 3f;
    public float dropSpeed = 5f;

    //particle system
    public ParticleSystem crumbParticles;

    //since i wanted the cookie to center itself
    [Header("Center Adjustment")]
    public float leftShiftAmount = 0.35f;
    public float dragLeftShiftAmount = 0.15f;
    public float shiftSpeed = 5f;

    //drop tilt for the cookie
    [Header("Drop Tilt")]
    public float dropTiltAngle = -25f;
    public float dropTiltSpeed = 5f;

    //reveal canvas for the try again part
    [Header("Reveal Canvas")]
    public CanvasGroup revealOverlay;
    public TextMeshProUGUI revealFortuneText;
    public float delayBeforeOverlay = 2f;
    public float fadeSpeed = 2f;

    //auto open, triggered by a button instead of dragging
    [Header("Auto Open")]
    public float autoOpenSpeed = 2f;

    //all sfx related references
    [Header("SFX")]
    public AudioSource cookieBreakSFX;


    //everything below this are private variables
    [HideInInspector]
    public bool isActive = false;

    private bool isDragging;
    private bool isOpened;
    private bool isDropping;
    private bool isWaiting;
    private bool isFading;
    private bool isShifting;
    private bool isAutoOpening;
    

    private float waitTimer;
    private float dragDistance;

    private Camera cam;

    //which half is currently being dragged/broken off
    //and which one is just re-centering
    private Transform draggedHalf;
    private Transform anchorHalf;

    //+1 if dragging rightHalf
    //-1 if dragging leftHalf
    private float dragDirection = 1f;

    //captured when drag starts
    private Vector3 draggedBaseLocalPos;
    private Quaternion draggedBaseRot;
    private Vector3 anchorStartLocalPos;

    private Vector3 anchorTargetPos;
    private Vector3 paperTargetPos;
    private Vector3 dropTarget;

    //stores the original transform values so the cookie can completely reset
    private Transform originalRightParent;
    private Vector3 originalRightLocalPosition;
    private Quaternion originalRightLocalRotation;
    private Vector3 originalRightLocalScale;

    private Transform originalLeftParent;
    private Vector3 originalLeftLocalPosition;
    private Quaternion originalLeftLocalRotation;
    private Vector3 originalLeftLocalScale;

    private Vector3 originalPaperPosition;
    private Quaternion originalPaperRotation;
    private Vector3 originalPaperScale;

    private Vector3 originalPaperMaskScale;
    private Vector2 originalTextMaskSize;
    private Vector2 originalTextMaskAnchoredPos;

    //paper's original parent
    private Transform originalFortunePaperParent;

    //original relationship between paper and left cookie
    private Vector3 paperOffsetFromLeftAnchor;

    //used so the paper is only repositioned once when left dragging starts
    private bool paperRepositionedForLeftDrag;

    //the paper's position right after PositionPaperForLeftDrag runs, before
    //any progressive open-offset is layered on top each frame
    private Vector3 leftDragBasePaperPosition;


    void Start()
    {
        cam = Camera.main;

        anchorTargetPos = leftHalf.position;
        paperTargetPos = fortunePaperTransform.position;

        revealOverlay.alpha = 0f;
        revealOverlay.interactable = false;
        revealOverlay.blocksRaycasts = false;

        cookieBreakSFX = GetComponent<AudioSource>();

        if (fortuneTextObject != null)
        {
            fortuneTextObject.SetActive(false);
        }

        //stores original right cookie transform
        originalRightParent = rightHalf.parent;
        originalRightLocalPosition = rightHalf.localPosition;
        originalRightLocalRotation = rightHalf.localRotation;
        originalRightLocalScale = rightHalf.localScale;

        //stores original left cookie transform
        originalLeftParent = leftHalf.parent;
        originalLeftLocalPosition = leftHalf.localPosition;
        originalLeftLocalRotation = leftHalf.localRotation;
        originalLeftLocalScale = leftHalf.localScale;

        //stores original paper transform
        originalPaperPosition = fortunePaperTransform.position;
        originalPaperRotation = fortunePaperTransform.rotation;
        originalPaperScale = fortunePaperTransform.localScale;

        //stores original paper parent
        originalFortunePaperParent = fortunePaperTransform.parent;

        //original paper offset from left cookie
        paperOffsetFromLeftAnchor =
            fortunePaperTransform.position - leftHalf.position;

        //stores original paper mask scale
        originalPaperMaskScale = paperMaskTransform.localScale;

        //stores original text mask size and anchored position
        if (textMaskRect != null)
        {
            originalTextMaskSize = textMaskRect.sizeDelta;
            originalTextMaskAnchoredPos = textMaskRect.anchoredPosition;
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
        t.position = Vector3.MoveTowards(
            t.position,
            target,
            speed * Time.deltaTime
        );

        return Vector3.Distance(t.position, target) < 0.01f;
    }


    public void SpawnCrumbs(Vector2 position)
    {
        if (crumbParticles == null) return;

        crumbParticles.transform.position = position;
        crumbParticles.Play();
    }


    void HandleShifting()
    {
        bool anchorDone = MoveToward(
            anchorHalf,
            anchorTargetPos,
            shiftSpeed
        );

        fortunePaperTransform.position =
            Vector3.MoveTowards(
                fortunePaperTransform.position,
                paperTargetPos,
                shiftSpeed * Time.deltaTime
            );

        if (anchorDone)
        {
            isShifting = false;
        }
    }


    void HandleDropping()
    {
        bool draggedDone = MoveToward(
            draggedHalf,
            dropTarget,
            dropSpeed
        );

        draggedHalf.rotation = Quaternion.Lerp(
            draggedHalf.rotation,
            draggedBaseRot *
            Quaternion.Euler(
                0,
                0,
                dropTiltAngle * dragDirection
            ),
            dropTiltSpeed * Time.deltaTime
        );

        if (draggedDone)
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

            if (revealOverlay != null)
            {
                revealOverlay.interactable = true;
                revealOverlay.blocksRaycasts = true;
            }
        }
    }


    void HandleFading()
    {
        revealOverlay.alpha = Mathf.MoveTowards(
            revealOverlay.alpha,
            1f,
            fadeSpeed * Time.deltaTime
        );
    }


    public void ActivateFortune()
    {
        if (fortuneText != null && fortuneDatabase != null)
        {
            fortuneText.text =
                fortuneDatabase.GetRandomFortune();

            fortuneText.maxVisibleCharacters =
                fortuneText.text.Length;
        }

        SetMasks(
            closedTextMaskWidth,
            closedPaperScaleX,
            closedPaperMaskScaleX
        );

        if (fortuneTextObject != null)
        {
            fortuneTextObject.SetActive(true);
        }
    }


    public void AutoOpen()
    {
        if (isOpened || isAutoOpening) return;

        isDragging = false;

        SetDefaultDragTarget();

        isAutoOpening = true;
    }


    public void ForceOpen()
    {
        if (isOpened) return;

        SetDefaultDragTarget();

        dragDistance = openThreshold;

        CheckOpen();
    }


    void SetDefaultDragTarget()
    {
        if (draggedHalf != null) return;

        draggedHalf = rightHalf;
        anchorHalf = leftHalf;
        dragDirection = 1f;

        draggedBaseLocalPos =
            rightHalf.localPosition;

        draggedBaseRot =
            rightHalf.localRotation;

        anchorStartLocalPos =
            leftHalf.localPosition;

        //Place the fortune text mask according to whichever
        //direction slot is set (default direction is right).
        PositionTextMaskForDirection();
    }


    //IMPORTANT:
    //
    //The paper stays visually attached to the cookie half
    //that is NOT being dragged.
    //
    //RIGHT DRAG:
    //paper stays attached to LEFT half.
    //
    //LEFT DRAG:
    //paper stays attached to RIGHT half.
    //
    //The paper is NOT re-parented.
    void PositionPaperForLeftDrag()
    {
        if (paperRepositionedForLeftDrag)
            return;

        if (fortunePaperTransform == null ||
            rightHalf == null)
            return;

        //The LEFT half is being dragged,
        //so the RIGHT half becomes the paper's visual anchor.
        //
        //Calculate the original paper offset from the LEFT half,
        //then mirror that relationship so it sits on the RIGHT side.
        Vector3 mirroredOffset =
            new Vector3(
                -paperOffsetFromLeftAnchor.x,
                paperOffsetFromLeftAnchor.y,
                paperOffsetFromLeftAnchor.z
            );

        //Add the editable left-drag adjustment.
        //
        //Negative values move the paper to the LEFT.
        //Positive values move the paper to the RIGHT.
        mirroredOffset.x += leftDragPaperOffsetX;

        //Place the paper relative to the RIGHT cookie.
        fortunePaperTransform.position =
            rightHalf.position +
            mirroredOffset;

        //Match the paper orientation to the cookie.
        fortunePaperTransform.rotation =
            rightHalf.rotation;

        //The actual paper sprite follows the RIGHT cookie orientation.
        if (fortunePaperScaleTarget != null)
        {
            fortunePaperScaleTarget.rotation =
                rightHalf.rotation;
        }

        //The paper mask follows the RIGHT cookie orientation.
        if (paperMaskTransform != null)
        {
            paperMaskTransform.rotation =
                rightHalf.rotation;
        }

        paperRepositionedForLeftDrag = true;

        //remember this as the base - the progressive open-offset gets
        //layered on top of this each frame, not baked into it here
        leftDragBasePaperPosition =
            fortunePaperTransform.position;

        //The paper should remain centered around this
        //new right-side anchor during the opening shift.
        paperTargetPos =
            fortunePaperTransform.position;
    }


    //Sets the fortune text mask's anchored position (Pos X / Pos Y) to whichever
    //value matches the current drag direction.
    void PositionTextMaskForDirection()
    {
        if (textMaskRect == null) return;

        textMaskRect.anchoredPosition =
            (dragDirection > 0f)
            ? textMaskPosRight
            : textMaskPosLeft;
    }


    void HandleDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos =
                cam.ScreenToWorldPoint(
                    Input.mousePosition
                );

            Collider2D hit =
                Physics2D.OverlapPoint(
                    mousePos,
                    draggableLayer
                );

            if (hit != null &&
                (hit.transform == rightHalf ||
                 hit.transform == leftHalf))
            {
                draggedHalf = hit.transform;

                anchorHalf =
                    (draggedHalf == rightHalf)
                    ? leftHalf
                    : rightHalf;

                dragDirection =
                    (draggedHalf == rightHalf)
                    ? 1f
                    : -1f;

                draggedBaseLocalPos =
                    draggedHalf.localPosition;

                draggedBaseRot =
                    draggedHalf.localRotation;

                anchorStartLocalPos =
                    anchorHalf.localPosition;

                //If the LEFT cookie is grabbed,
                //reposition the paper immediately.
                //
                //The paper stays in its original hierarchy.
                if (draggedHalf == leftHalf)
                {
                    PositionPaperForLeftDrag();
                }
                else
                {
                    //RIGHT cookie is being dragged,
                    //so the LEFT cookie is the paper anchor.
                    //
                    //Keep the paper attached to the LEFT side.
                    paperTargetPos =
                        fortunePaperTransform.position;
                }

                //Place the fortune text mask according to whichever
                //direction slot is set for this drag direction.
                PositionTextMaskForDirection();

                SpawnCrumbs(
                    transform.position +
                    Vector3.down * 0.2f
                );

                isDragging = true;

                if (cookieBreakSFX != null)
                {
                    cookieBreakSFX.Play();
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (!isDragging) return;

        Vector3 mouseWorld =
            cam.ScreenToWorldPoint(
                Input.mousePosition
            );

        mouseWorld.z = 0;

        Vector3 mouseLocal =
            draggedHalf.parent.InverseTransformPoint(
                mouseWorld
            );

        float dragAmount =
            Mathf.Max(
                (mouseLocal.x -
                 draggedBaseLocalPos.x) *
                dragDirection,
                0
            );

        draggedHalf.localPosition =
            new Vector3(
                draggedBaseLocalPos.x +
                dragAmount * dragDirection,
                draggedBaseLocalPos.y,
                draggedBaseLocalPos.z
            );

        draggedHalf.localRotation =
            draggedBaseRot;

        dragDistance = dragAmount;

        float dragT =
            Mathf.InverseLerp(
                0,
                openThreshold,
                dragDistance
            );

        anchorHalf.localPosition =
            Vector3.Lerp(
                anchorStartLocalPos,
                anchorStartLocalPos +
                Vector3.left *
                dragDirection *
                dragLeftShiftAmount,
                dragT
            );

        UpdatePaper(dragT);

        CheckOpen();
    }


    void HandleAutoOpen()
    {
        dragDistance +=
            autoOpenSpeed *
            Time.deltaTime;

        dragDistance =
            Mathf.Min(
                dragDistance,
                openThreshold
            );

        draggedHalf.localPosition =
            new Vector3(
                draggedBaseLocalPos.x +
                dragDistance * dragDirection,
                draggedBaseLocalPos.y,
                draggedBaseLocalPos.z
            );

        draggedHalf.localRotation =
            draggedBaseRot;

        float dragT =
            Mathf.InverseLerp(
                0,
                openThreshold,
                dragDistance
            );

        anchorHalf.localPosition =
            Vector3.Lerp(
                anchorStartLocalPos,
                anchorStartLocalPos +
                Vector3.left *
                dragDirection *
                dragLeftShiftAmount,
                dragT
            );

        UpdatePaper(dragT);

        if (dragDistance >= openThreshold)
        {
            isAutoOpening = false;
            CheckOpen();
        }
    }


    //Applies the progressive left-drag offset, scaling from 0 at the start
    //of the drag up to leftDragPaperOpenOffsetX at full open (t=1). Only
    //has an effect for the left-drag case - right-drag never touches
    //fortunePaperTransform's position here.
    void ApplyLeftDragOpenOffset(float t)
    {
        if (dragDirection >= 0f) return;
        if (fortunePaperTransform == null) return;
        if (!paperRepositionedForLeftDrag) return;

        fortunePaperTransform.position =
            leftDragBasePaperPosition +
            new Vector3(leftDragPaperOpenOffsetX * t, 0f, 0f);
    }


    void UpdatePaper(float t)
    {
        float textWidth =
            Mathf.Lerp(
                closedTextMaskWidth,
                openTextMaskWidth,
                t
            );

        float paperScale =
            Mathf.Lerp(
                closedPaperScaleX,
                paperOpenScaleX,
                t
            );

        float paperMaskScale =
            Mathf.Lerp(
                closedPaperMaskScaleX,
                paperMaskOpenScaleX,
                t
            );

        SetMasks(
            textWidth,
            paperScale,
            paperMaskScale
        );

        ApplyLeftDragOpenOffset(t);
    }


    void SetMasks(
        float textWidth,
        float paperScaleX,
        float paperMaskScaleX
    )
    {
        //Paper mask can flip direction.
        Vector3 maskScale =
            paperMaskTransform.localScale;

        maskScale.x =
            paperMaskScaleX *
            dragDirection;

        paperMaskTransform.localScale =
            maskScale;

        //Actual paper can flip direction.
        if (fortunePaperScaleTarget != null)
        {
            Vector3 paperScale =
                fortunePaperScaleTarget.localScale;

            paperScale.x =
                paperScaleX *
                dragDirection;

            fortunePaperScaleTarget.localScale =
                paperScale;
        }

        //TEXT MASK MUST ALWAYS HAVE POSITIVE WIDTH.
        if (textMaskRect != null)
        {
            Vector2 size =
                textMaskRect.sizeDelta;

            size.x = textWidth;

            textMaskRect.sizeDelta =
                size;
        }
    }


    void CheckOpen()
    {
        if (dragDistance < openThreshold)
            return;

        isOpened = true;
        isDragging = false;

        SetMasks(
            openTextMaskWidth,
            paperOpenScaleX,
            paperMaskOpenScaleX
        );

        //Anchor cookie re-centers itself.
        anchorTargetPos =
            anchorHalf.position +
            Vector3.left *
            dragDirection *
            leftShiftAmount;

        //RIGHT DRAG:
        //
        //Paper gets its normal centering shift.
        if (dragDirection > 0f)
        {
            paperTargetPos =
                fortunePaperTransform.position +
                Vector3.left *
                dragDirection *
                leftShiftAmount;
        }

        //LEFT DRAG:
        //
        //Paper was already positioned relative to the
        //right cookie when the drag started.
        else
        {
            Vector3 anchorMovement =
                anchorTargetPos -
                anchorHalf.position;

            paperTargetPos =
                fortunePaperTransform.position +
                anchorMovement;
        }

        isShifting = true;

        Vector3 currentDraggedPos =
            draggedHalf.position;

        draggedHalf.SetParent(
            null,
            true
        );

        draggedHalf.position =
            currentDraggedPos;

        //Throws dragged cookie half away.
        dropTarget =
            currentDraggedPos +
            new Vector3(
                throwRightDistance *
                dragDirection,
                -dropDistance,
                0
            );

        isDropping = true;
    }


    public void ResetCookie()
    {
        //stops all cookie states
        isActive = false;
        isDragging = false;
        isOpened = false;
        isDropping = false;
        isWaiting = false;
        isFading = false;
        isShifting = false;
        isAutoOpening = false;

        //resets timers
        waitTimer = 0f;
        dragDistance = 0f;

        //allows paper repositioning again next round
        paperRepositionedForLeftDrag = false;

        //puts right cookie back under original parent
        if (rightHalf != null)
        {
            rightHalf.SetParent(
                originalRightParent,
                false
            );

            rightHalf.localPosition =
                originalRightLocalPosition;

            rightHalf.localRotation =
                originalRightLocalRotation;

            rightHalf.localScale =
                originalRightLocalScale;
        }

        //resets left cookie, including re-parenting it back in case it was the
        //one that broke off and got detached (SetParent(null,...)) in CheckOpen
        if (leftHalf != null)
        {
            leftHalf.SetParent(
                originalLeftParent,
                false
            );

            leftHalf.localPosition =
                originalLeftLocalPosition;

            leftHalf.localRotation =
                originalLeftLocalRotation;

            leftHalf.localScale =
                originalLeftLocalScale;
        }

        //IMPORTANT:
        //
        //Paper is always restored to its original parent.
        if (fortunePaperTransform != null)
        {
            fortunePaperTransform.SetParent(
                originalFortunePaperParent,
                true
            );

            fortunePaperTransform.position =
                originalPaperPosition;

            fortunePaperTransform.rotation =
                originalPaperRotation;

            fortunePaperTransform.localScale =
                originalPaperScale;
        }

        //resets paper mask
        if (paperMaskTransform != null)
        {
            paperMaskTransform.localScale =
                originalPaperMaskScale;
        }

        //resets text mask size and anchored position
        if (textMaskRect != null)
        {
            textMaskRect.sizeDelta =
                originalTextMaskSize;

            textMaskRect.anchoredPosition =
                originalTextMaskAnchoredPos;
        }

        //closed state
        SetMasks(
            closedTextMaskWidth,
            closedPaperScaleX,
            closedPaperMaskScaleX
        );

        //hides fortune text
        if (fortuneTextObject != null)
        {
            fortuneTextObject.SetActive(false);
        }

        //clears fortune text
        if (fortuneText != null)
        {
            fortuneText.text = "";
        }

        //clears revealed fortune text
        if (revealFortuneText != null)
        {
            revealFortuneText.text = "";
        }

        //resets reveal overlay
        if (revealOverlay != null)
        {
            revealOverlay.alpha = 0f;
            revealOverlay.interactable = false;
            revealOverlay.blocksRaycasts = false;
        }

        //resets target positions
        anchorTargetPos =
            leftHalf.position;

        paperTargetPos =
            fortunePaperTransform.position;

        //clears drag state
        draggedHalf = null;
        anchorHalf = null;
        dragDirection = 1f;
    }
}