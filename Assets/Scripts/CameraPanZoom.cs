using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraPanZoom : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private SpriteRenderer drawingRenderer;

    [Header("Zoom")]
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 15f;
    [SerializeField] private float zoomSpeed = 5f;

    [Header("Zoom Animation")]
    [SerializeField] private float zoomSmoothSpeed = 10f;

    [Header("Pan")]
    [SerializeField] private float panSpeed = 1f;
    [SerializeField] private float panSmoothSpeed = 15f;
    private Vector3 cameraVelocity;


    [Header("Clamp")]
    [SerializeField] private float edgePadding = 0.25f;

    [Header("Tap Detection")]
    [SerializeField] private float tapThreshold = 10f;

    [Header("Double Tap")]
    [SerializeField] private float doubleTapTime = 0.3f;

    private float lastTapTime = -1f;

    private Vector2 pointerDownScreenPos;
    public bool WasTapThisFrame { get; private set; }

    public static bool IsDragging { get; private set; }
    public static CameraPanZoom Instance { get; private set; }
    public Vector2 TapScreenPosition { get; private set; }

    
    private float defaultZoom;
    private float targetZoom;
    private Vector3 targetPosition;
    private Bounds drawingBounds;
    private bool isPanning;
    private Vector3 dragStartWorld;

    private float lastPinchDistance;
    private bool isPinching;

    private void Awake()
    {
        Instance = this;
        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();

        FitToDrawing();
    }

    public void FitToDrawing()
    {
        if (drawingRenderer == null || drawingRenderer.sprite == null)
            return;

        drawingBounds = drawingRenderer.bounds;

        float spriteHeight = drawingBounds.size.y;
        float spriteWidth = drawingBounds.size.x;

        float screenAspect = (float)Screen.width / Screen.height;

        float verticalSize = spriteHeight * 0.5f;
        float horizontalSize = spriteWidth * 0.5f / screenAspect;

        defaultZoom = Mathf.Max(verticalSize, horizontalSize);

        targetCamera.orthographicSize = defaultZoom;
        targetZoom = defaultZoom;
        targetPosition = targetCamera.transform.position;

        // Dynamic zoom limits
        maxZoom = defaultZoom;
        minZoom = defaultZoom * 0.30f;
    }

    private void Update()
        {
            WasTapThisFrame = false;

    #if UNITY_EDITOR || UNITY_STANDALONE
            HandleMouseZoom();
            HandleMousePan();
    #endif

    #if UNITY_ANDROID || UNITY_IOS
            HandleTouchInput();
    #endif

        SmoothZoom();   
        SmoothPosition(); // Mobile pinch and pan will be added next.
        }

    #if UNITY_EDITOR || UNITY_STANDALONE

        private void HandleMouseZoom()
        {
            float scroll = Input.mouseScrollDelta.y;

            if (Mathf.Abs(scroll) < 0.01f)
                return;

            targetZoom -= scroll * zoomSpeed;

            targetZoom = Mathf.Clamp(
                targetZoom,
                minZoom,
                maxZoom);
                ClampCamera();
            if (targetZoom >= maxZoom - 0.01f)
            {
                ResetView();
            }
        }
        private void HandleMousePan()
        {
            // Don't allow panning when fully zoomed out

            if (Input.GetMouseButtonDown(0))
            {
                pointerDownScreenPos = Input.mousePosition;

                dragStartWorld = targetCamera.ScreenToWorldPoint(pointerDownScreenPos);

                isPanning = false;
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (!isPanning)
                {
                    if (targetCamera.orthographicSize >= defaultZoom - 0.02f)
                    return;
                    
                    if (IsDoubleTap())
                    {
                        ResetView();
                    }
                    else
                    {
                        TapScreenPosition = Input.mousePosition;
                        WasTapThisFrame = true;
                    }
                }

                // Don't pan when fully zoomed out.
                

                isPanning = false;
                IsDragging = false;
            }

            if (Input.GetMouseButton(0))
            {
                float distance =
                    Vector2.Distance(pointerDownScreenPos, Input.mousePosition);

                if (!isPanning && distance > tapThreshold)
                {
                    isPanning = true;
                    IsDragging = true;
                }

                if (!isPanning)
                    return;
                Vector3 currentWorld = targetCamera.ScreenToWorldPoint(Input.mousePosition);

                Vector3 delta = dragStartWorld - currentWorld;

                targetPosition += delta;

                ClampCamera();

                dragStartWorld = targetCamera.ScreenToWorldPoint(Input.mousePosition);
            
            }
        }
        #endif

        private void HandleTouchInput()
        {
            if (Input.touchCount == 2)
            {
                HandlePinchZoom();
                return;
            }

            if (Input.touchCount == 1)
            {
                HandleSingleFinger();
            }

            if (Input.touchCount < 2)
            {
                isPinching = false;
            }
        }
        private void HandlePinchZoom()
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            float currentDistance = Vector2.Distance(
                touch0.position,
                touch1.position);

            if (!isPinching)
            {
                lastPinchDistance = currentDistance;
                isPinching = true;
                return;
            }

            float delta = currentDistance - lastPinchDistance;

            targetZoom -= delta * 0.01f;

            targetZoom = Mathf.Clamp(
                targetZoom,
                minZoom,
                maxZoom);

            ClampCamera();

            if (targetZoom >= maxZoom - 0.01f)
            {
                ResetView();
            }

            lastPinchDistance = currentDistance;
        }
       private void HandleSingleFinger()
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:

                    pointerDownScreenPos = touch.position;
                    dragStartWorld = targetCamera.ScreenToWorldPoint(touch.position);

                    isPanning = false;
                    break;

                case TouchPhase.Moved:
                
                    

                    float distance =
                        Vector2.Distance(pointerDownScreenPos, touch.position);

                    if (!isPanning && distance > tapThreshold)
                    {
                        isPanning = true;
                        IsDragging = true;
                    }

                    if (isPanning)
                    {
                        if (targetCamera.orthographicSize >= defaultZoom - 0.02f)
                        return;

                        Vector3 currentWorld =
                            targetCamera.ScreenToWorldPoint(touch.position);

                        Vector3 delta = dragStartWorld - currentWorld;

                        targetPosition += delta;

                        ClampCamera();

                        dragStartWorld =
                            targetCamera.ScreenToWorldPoint(touch.position);
                    }

                    break;

                case TouchPhase.Ended:

                    if (!isPanning)
                    {
                        if (IsDoubleTap())
                        {
                            ResetView();
                        }
                        else
                        {
                            TapScreenPosition = touch.position;
                            WasTapThisFrame = true;
                        }
                    }

                    isPanning = false;
                    IsDragging = false;
                    break;
            }
        }

        private void ClampCamera()
            {
                if (drawingRenderer == null || drawingRenderer.sprite == null)
                    return;

                Bounds bounds = drawingBounds;

                float camHeight = targetCamera.orthographicSize;
                float camWidth = camHeight * targetCamera.aspect;

                float minX = bounds.min.x + camWidth - edgePadding;
                float maxX = bounds.max.x - camWidth + edgePadding;

                float minY = bounds.min.y + camHeight - edgePadding;
                float maxY = bounds.max.y - camHeight + edgePadding;

                Vector3 pos = targetPosition;

                // Drawing smaller than screen
                if (minX > maxX)
                    pos.x = bounds.center.x;
                else
                    pos.x = Mathf.Clamp(pos.x, minX, maxX);

                if (minY > maxY)
                    pos.y = bounds.center.y;
                else
                    pos.y = Mathf.Clamp(pos.y, minY, maxY);

                targetPosition = new Vector3(
                    pos.x,
                    pos.y,
                    targetCamera.transform.position.z);
            }
        

    

    public void ResetView()
    {
        drawingBounds = drawingRenderer.bounds;

        targetZoom = defaultZoom;

        targetPosition =
        new Vector3(
            drawingBounds.center.x,
            drawingBounds.center.y,
            targetCamera.transform.position.z);

        ClampCamera();
    }

    private bool IsDoubleTap()
    {
        if (Time.time - lastTapTime <= doubleTapTime)
        {
            lastTapTime = -1f;
            return true;
        }

        lastTapTime = Time.time;
        return false;
    }
    private void SmoothZoom()
    {
        targetCamera.orthographicSize = Mathf.Lerp(
            targetCamera.orthographicSize,
            targetZoom,
            Time.deltaTime * zoomSmoothSpeed);

        ClampCamera();
    }
    private void SmoothPosition()
    {
        targetCamera.transform.position = Vector3.SmoothDamp(
            targetCamera.transform.position,
            targetPosition,
            ref cameraVelocity,
            0.08f);
    }
}