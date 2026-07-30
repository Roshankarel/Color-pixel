using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    #region References

    [Header("References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private SpriteRenderer drawingRenderer;

    #endregion

    #region Zoom

    [Header("Zoom")]

    [SerializeField] private float zoomSpeed = 5f;

    [SerializeField] private float zoomSmoothTime = 0.08f;

    [SerializeField] private float minZoomMultiplier = 0.30f;

    #endregion

    #region Pan

    [Header("Pan")]

    [SerializeField] private float panSmoothTime = 0.08f;

    [SerializeField] private float tapThreshold = 10f;

    #endregion

    #region Clamp

    [Header("Clamp")]

    [SerializeField] private float edgePadding = 0.25f;

    #endregion

    #region Double Tap

    [Header("Double Tap")]

    [SerializeField] private float doubleTapTime = 0.30f;

    #endregion

    #region Runtime

    public static CameraController Instance { get; private set; }

    public static bool IsDragging { get; private set; }

    public bool WasTapThisFrame { get; private set; }

    public Vector2 TapScreenPosition { get; private set; }

    private Bounds drawingBounds;

    private float defaultZoom;

    private float minZoom;

    private float maxZoom;

    private float targetZoom;

    private float zoomVelocity;

    private Vector3 targetPosition;

    private Vector3 moveVelocity;

    private bool isPanning;

    private bool isPinching;

    private float lastTapTime = -1f;

    private float lastPinchDistance;

    private Vector2 pointerDownScreenPos;

    private Vector3 dragStartWorld;

    #endregion

    private void Awake()
    {
        Instance = this;

        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();

        FitToDrawing();
    }

    private void Update()
    {
        WasTapThisFrame = false;

#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseInput();
#endif

#if UNITY_ANDROID || UNITY_IOS
        HandleTouchInput();
#endif

        SmoothZoom();

        SmoothPosition();

        ClampCamera();
    }
    #region Public API

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

    maxZoom = defaultZoom;
    minZoom = defaultZoom * minZoomMultiplier;

    targetZoom = defaultZoom;

    targetPosition = new Vector3(
        drawingBounds.center.x,
        drawingBounds.center.y,
        targetCamera.transform.position.z);

    targetCamera.orthographicSize = defaultZoom;
    targetCamera.transform.position = targetPosition;
}

public void ResetView(bool animated = true)
{
    MoveTo(drawingBounds.center, animated);
    ZoomTo(defaultZoom, animated);
}

public void MoveTo(Vector2 worldPosition, bool animated = true)
{
    Vector3 pos = new Vector3(
        worldPosition.x,
        worldPosition.y,
        targetCamera.transform.position.z);

    if (animated)
    {
        targetPosition = pos;
    }
    else
    {
        targetPosition = pos;
        targetCamera.transform.position = pos;
    }
}

public void ZoomTo(float zoom, bool animated = true)
{
    zoom = Mathf.Clamp(zoom, minZoom, maxZoom);

    if (animated)
    {
        targetZoom = zoom;
    }
    else
    {
        targetZoom = zoom;
        targetCamera.orthographicSize = zoom;
    }
}

public void FocusOnPoint(Vector2 point, float zoom = -1f)
{
    MoveTo(point);

    if (zoom > 0)
        ZoomTo(zoom);
}

public void FocusOnBounds(Bounds bounds)
{
    float aspect = targetCamera.aspect;

    float vertical = bounds.size.y * 0.5f;

    float horizontal = bounds.size.x * 0.5f / aspect;

    float zoom = Mathf.Max(vertical, horizontal);

    zoom = Mathf.Clamp(zoom, minZoom, maxZoom);

    MoveTo(bounds.center);

    ZoomTo(zoom);
}

#endregion

#region Camera Animation

private void SmoothZoom()
{
    targetCamera.orthographicSize =
        Mathf.SmoothDamp(
            targetCamera.orthographicSize,
            targetZoom,
            ref zoomVelocity,
            zoomSmoothTime);
}

private void SmoothPosition()
{
    targetCamera.transform.position =
        Vector3.SmoothDamp(
            targetCamera.transform.position,
            targetPosition,
            ref moveVelocity,
            panSmoothTime);
}

#endregion
#region Clamp

private void ClampCamera()
{
    if (drawingRenderer == null)
        return;

    float camHeight = targetCamera.orthographicSize;
    float camWidth = camHeight * targetCamera.aspect;

    float minX = drawingBounds.min.x + camWidth - edgePadding;
    float maxX = drawingBounds.max.x - camWidth + edgePadding;

    float minY = drawingBounds.min.y + camHeight - edgePadding;
    float maxY = drawingBounds.max.y - camHeight + edgePadding;

    Vector3 pos = targetPosition;

    if (minX > maxX)
        pos.x = drawingBounds.center.x;
    else
        pos.x = Mathf.Clamp(pos.x, minX, maxX);

    if (minY > maxY)
        pos.y = drawingBounds.center.y;
    else
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

    targetPosition = new Vector3(
        pos.x,
        pos.y,
        targetPosition.z);
}

#endregion

#region Mouse Input

private void HandleMouseInput()
{
    HandleMouseZoom();

    if (Input.GetMouseButtonDown(0))
    {
        BeginPan(Input.mousePosition);
    }

    if (Input.GetMouseButton(0))
    {
        UpdatePan(Input.mousePosition);
    }

    if (Input.GetMouseButtonUp(0))
    {
        EndPan(Input.mousePosition);
    }
}
private void HandleMouseZoom()
{
    float scroll = Input.mouseScrollDelta.y;

    if (Mathf.Abs(scroll) < 0.01f)
        return;

    ZoomAtScreenPoint(
    Input.mousePosition,
    -scroll * zoomSpeed);

    if (targetZoom >= maxZoom - 0.01f)
    {
        ResetView();
    }
}

#endregion


#region Touch Input

private void HandleTouchInput()
{
    if (Input.touchCount == 0)
    {
        isPinching = false;
        return;
    }

    // ---------- Pinch ----------
    if (Input.touchCount == 2)
    {
        Touch finger0 = Input.GetTouch(0);
        Touch finger1 = Input.GetTouch(1);

        float currentDistance =
            Vector2.Distance(finger0.position, finger1.position);

        if (!isPinching)
        {
            isPinching = true;
            isPanning = false;
            IsDragging = false;

            lastPinchDistance = currentDistance;
            return;
        }

        Vector2 pinchCenter =
        (finger0.position + finger1.position) * 0.5f;

        float delta = currentDistance - lastPinchDistance;

        ZoomAtScreenPoint(
            pinchCenter,
            -delta * 0.01f);

        lastPinchDistance = currentDistance;

        return;
    }

    // ---------- Single Finger ----------
    isPinching = false;

    Touch touch = Input.GetTouch(0);

    switch (touch.phase)
    {
        case TouchPhase.Began:
            BeginPan(touch.position);
            break;

        case TouchPhase.Moved:
        case TouchPhase.Stationary:
            UpdatePan(touch.position);
            break;

        case TouchPhase.Ended:
        case TouchPhase.Canceled:
            EndPan(touch.position);
            break;
    }
}
#endregion


#region Shared Input
private void BeginPan(Vector2 screenPosition)
{
    pointerDownScreenPos = screenPosition;

    dragStartWorld = targetCamera.ScreenToWorldPoint(screenPosition);

    isPanning = false;
}
private void UpdatePan(Vector2 screenPosition)
{
    float distance =
        Vector2.Distance(pointerDownScreenPos, screenPosition);

    if (!isPanning && distance > tapThreshold)
    {
        isPanning = true;
        IsDragging = true;
    }

    if (!isPanning)
        return;

    if (targetCamera.orthographicSize >= defaultZoom - 0.02f)
        return;

    Vector3 currentWorld =
        targetCamera.ScreenToWorldPoint(screenPosition);

    Vector3 delta = dragStartWorld - currentWorld;

    targetPosition += delta;

    dragStartWorld = currentWorld;
}

private void EndPan(Vector2 screenPosition)
{
    if (!isPanning)
    {
        if (IsDoubleTap())
        {
            ResetView();
        }
        else
        {
            TapScreenPosition = screenPosition;
            WasTapThisFrame = true;
        }
    }

    isPanning = false;
    IsDragging = false;
}
private void ZoomAtScreenPoint(Vector2 screenPoint, float zoomDelta)
{
    float oldZoom = targetZoom;

    float newZoom = Mathf.Clamp(
        oldZoom + zoomDelta,
        minZoom,
        maxZoom);

    if (Mathf.Approximately(oldZoom, newZoom))
        return;

    Vector3 worldPoint = targetCamera.ScreenToWorldPoint(screenPoint);

    float zoomFactor = newZoom / oldZoom;

    Vector3 offset = (worldPoint - targetPosition) * (1f - zoomFactor);

    targetPosition += offset;

    targetZoom = newZoom;
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
#endregion
}