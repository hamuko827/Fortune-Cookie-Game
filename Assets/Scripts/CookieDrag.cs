using UnityEngine;
using TMPro;

public class CookieDrag : MonoBehaviour
{
    [Header("References")]
    public Transform leftHalf;
    public Transform rightHalf;
    public Transform fortunePaperTransform;

    public FortuneDatabase fortuneDatabase;
    public TextMeshProUGUI fortuneText;
    public GameObject fortuneTextObject;
    public Transform fortuneTextTransform;

    [Header("Fortune Text Position")]
    public Vector3 fortuneTextOffset;


    [Header("Dragging")]
    public LayerMask draggableLayer;
    public float openThreshold = 1.5f;
    public float maxTiltAngle = 15f;


    [Header("Paper")]
    public float closedPaperScaleX = 0.6f;
    public float paperOpenScaleX = 2f;


    [Header("Break")]
    public float throwRightDistance = 2f;
    public float dropDistance = 3f;
    public float dropSpeed = 5f;


    [Header("Center Adjustment")]
    public float leftShiftAmount = 0.35f;
    public float dragLeftShiftAmount = 0.15f;
    public float shiftSpeed = 5f;


    [Header("Drop Tilt")]
    public float dropTiltAngle = -25f;
    public float dropTiltSpeed = 5f;


    [Header("Overlay")]
    public CanvasGroup revealOverlay;
    public float delayBeforeOverlay = 2f;
    public float fadeSpeed = 2f;


    [HideInInspector]
    public bool isActive = false;


    private bool isDragging;
    private bool isOpened;
    private bool isDropping;
    private bool isWaiting;
    private bool isFading;
    private bool isShifting;


    private float waitTimer;


    private Vector3 leftDragStartLocalPos;

    private Vector3 rightBaseLocalPos;
    private Quaternion rightBaseRot;


    private Vector3 leftTargetPos;
    private Vector3 paperTargetPos;


    private float dragDistance;

    private Vector3 dropTarget;

    void Start()
    {
        leftDragStartLocalPos = leftHalf.localPosition;

        rightBaseLocalPos = rightHalf.localPosition;
        rightBaseRot = rightHalf.localRotation;

        leftTargetPos = leftHalf.position;
        paperTargetPos = fortunePaperTransform.position;
        revealOverlay.alpha = 0f;

        if (fortuneTextObject != null)
        {
            fortuneTextObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!isActive)
            return;
        FollowFortunePaper();

        if (isShifting)
        {
            leftHalf.position = Vector3.MoveTowards(
                leftHalf.position,
                leftTargetPos,
                shiftSpeed * Time.deltaTime
            );

            fortunePaperTransform.position = Vector3.MoveTowards(
                fortunePaperTransform.position,
                paperTargetPos,
                shiftSpeed * Time.deltaTime
            );

            if (Vector3.Distance(leftHalf.position, leftTargetPos) < 0.01f)
            {
                isShifting = false;
            }
        }

        if (isDropping)
        {
            rightHalf.position = Vector3.MoveTowards(
                rightHalf.position,
                dropTarget,
                dropSpeed * Time.deltaTime
            );

            rightHalf.rotation = Quaternion.Lerp(
                rightHalf.rotation,
                rightBaseRot * Quaternion.Euler(0, 0, dropTiltAngle),
                dropTiltSpeed * Time.deltaTime
            );

            if (Vector3.Distance(rightHalf.position, dropTarget) < 0.01f)
            {
                isDropping = false;
                isWaiting = true;
                waitTimer = 0f;
            }
            return;
        }

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
            revealOverlay.alpha = Mathf.MoveTowards(
                revealOverlay.alpha,
                1f,
                fadeSpeed * Time.deltaTime
            );
            return;
        }

        if (isOpened)
            return;
        HandleDrag();
    }

    void FollowFortunePaper()
    {
        if (fortuneTextTransform != null && fortunePaperTransform != null)
        {
            fortuneTextTransform.position =
                fortunePaperTransform.TransformPoint(fortuneTextOffset);
            fortuneTextTransform.rotation =
                fortunePaperTransform.rotation;
        }
    }
    public void ActivateFortune()
    {
        if (fortuneText != null && fortuneDatabase != null)
        {
            fortuneText.text =
                fortuneDatabase.GetRandomFortune();
        }
        if (fortuneTextObject != null)
        {
            fortuneTextObject.SetActive(true);
        }
    }
    void HandleDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos =
                Camera.main.ScreenToWorldPoint(
                    Input.mousePosition
                );
            Collider2D hit =
                Physics2D.OverlapPoint(
                    mousePos,
                    draggableLayer
                );
            if (hit != null && hit.transform == rightHalf)
            {
                isDragging = true;
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
        if (!isDragging)
            return;
        Vector3 mouseWorld =
            Camera.main.ScreenToWorldPoint(
                Input.mousePosition
            );
        mouseWorld.z = 0;

        Vector3 mouseLocal =
            rightHalf.parent.InverseTransformPoint(mouseWorld);

        float dragAmount =
            mouseLocal.x - rightBaseLocalPos.x;
        dragAmount = Mathf.Max(
            dragAmount,
            0
        );

        rightHalf.localPosition =
            new Vector3(
                rightBaseLocalPos.x + dragAmount,
                rightBaseLocalPos.y,
                rightBaseLocalPos.z
            );
        rightHalf.localRotation = rightBaseRot;

        dragDistance = dragAmount;

        float dragT = Mathf.InverseLerp(
            0,
            openThreshold,
            dragDistance
        );
        leftHalf.localPosition = Vector3.Lerp(
            leftDragStartLocalPos,
            leftDragStartLocalPos + Vector3.left * dragLeftShiftAmount,
            dragT
        );
        UpdatePaper();
        CheckOpen();
    }

    void UpdatePaper()
    {
        float t =
            Mathf.InverseLerp(
                0,
                openThreshold,
                dragDistance
            );
        Vector3 scale =
            fortunePaperTransform.localScale;
        scale.x =
            Mathf.Lerp(
                closedPaperScaleX,
                paperOpenScaleX,
                t
            );
        fortunePaperTransform.localScale = scale;
    }
    void CheckOpen()
    {
        if (dragDistance < openThreshold)
            return;
        isOpened = true;
        isDragging = false;

        Vector3 scale =
            fortunePaperTransform.localScale;
        scale.x = paperOpenScaleX;
        fortunePaperTransform.localScale = scale;

        leftTargetPos =
            leftHalf.position +
            Vector3.left * leftShiftAmount;
        paperTargetPos =
            fortunePaperTransform.position +
            Vector3.left * leftShiftAmount;
        isShifting = true;

        Vector3 currentRightPos =
            rightHalf.position;
        rightHalf.SetParent(null, true);

        rightHalf.position =
            currentRightPos;

        dropTarget =
            currentRightPos +
            new Vector3(
                throwRightDistance,
                -dropDistance,
                0
            );

        isDropping = true;
        Debug.Log("Fortune Revealed!");
    }
}