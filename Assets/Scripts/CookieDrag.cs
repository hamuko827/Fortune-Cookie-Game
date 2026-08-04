using UnityEngine;

public class CookieDrag : MonoBehaviour
{
    public Transform leftHalf;
    public Transform rightHalf;
    public float openThreshold = 1.5f;

    public CanvasGroup revealOverlay;
    public float fadeSpeed = 2f;

    private bool isOpened = false;
    private bool isFading = false;
    public bool isActive = false; // only allow input once halves are ready

    void Update()
    {
        if (!isActive) return;

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
            rightHalf.position = new Vector3(mouseWorldPos.x, rightHalf.position.y, 0);
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

            leftHalf.position = new Vector3(touch1WorldPos.x, leftHalf.position.y, 0);
            rightHalf.position = new Vector3(touch2WorldPos.x, rightHalf.position.y, 0);
            CheckOpenDistance();
        }
    }

    void CheckOpenDistance()
    {
        float distance = Vector3.Distance(leftHalf.position, rightHalf.position);
        if (distance >= openThreshold)
        {
            isOpened = true;
            isFading = true;
            Debug.Log("Fortune popped out, fading overlay in!");
        }
    }
}