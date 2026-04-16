using UnityEngine;

public class KidItemAnimator : MonoBehaviour
{
    [SerializeField] private float minSpeed = 25f;
    [SerializeField] private float maxSpeed = 50f;
    [SerializeField] private float directionChangeInterval = 1.5f;

    [Header("Boundary Padding (world space pixels)")]
    [SerializeField] private float paddingTop = 10f;
    [SerializeField] private float paddingBottom = 40f;
    [SerializeField] private float paddingLeft = 10f;
    [SerializeField] private float paddingRight = 10f;

    private RectTransform rectTransform;
    private RectTransform boundsRect;

    private Vector2 moveDirection;
    private float currentSpeed;
    private float directionTimer;
    private bool isMoving;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
            Debug.LogError($"{gameObject.name}: No RectTransform found on KidItemAnimator");
    }

    private void Update()
    {
        if (!isMoving || boundsRect == null) return;

        directionTimer -= Time.deltaTime;
        if (directionTimer <= 0f)
            PickRandomDirection();

        Vector3 worldPos = rectTransform.position;
        worldPos.x += moveDirection.x * currentSpeed * Time.deltaTime;
        worldPos.y += moveDirection.y * currentSpeed * Time.deltaTime;

        Vector3[] zoneCorners = new Vector3[4];
        boundsRect.GetWorldCorners(zoneCorners);

        float itemHalfW = rectTransform.rect.width * rectTransform.lossyScale.x / 2f;
        float itemHalfH = rectTransform.rect.height * rectTransform.lossyScale.y / 2f;

        float minX = zoneCorners[0].x + itemHalfW + paddingLeft;
        float maxX = zoneCorners[2].x - itemHalfW - paddingRight;
        float minY = zoneCorners[0].y + itemHalfH + paddingBottom;
        float maxY = zoneCorners[1].y - itemHalfH - paddingTop;

        if (minX > maxX) { float m = (minX + maxX) / 2f; minX = m; maxX = m; }
        if (minY > maxY) { float m = (minY + maxY) / 2f; minY = m; maxY = m; }

        if (worldPos.x > maxX) { worldPos.x = maxX; moveDirection.x = -Mathf.Abs(moveDirection.x); }
        if (worldPos.x < minX) { worldPos.x = minX; moveDirection.x = Mathf.Abs(moveDirection.x); }
        if (worldPos.y > maxY) { worldPos.y = maxY; moveDirection.y = -Mathf.Abs(moveDirection.y); }
        if (worldPos.y < minY) { worldPos.y = minY; moveDirection.y = Mathf.Abs(moveDirection.y); }

        rectTransform.position = worldPos;
    }

    public void StartMoving(RectTransform zone)
    {
        if (zone == null)
        {
            Debug.LogError($"{gameObject.name}: zone is null in StartMoving");
            return;
        }

        boundsRect = zone;
        isMoving = true;

        SnapInsideBounds();
        PickRandomDirection();
    }

    public void StopMoving()
    {
        isMoving = false;
    }

    private void SnapInsideBounds()
    {
        if (boundsRect == null) return;

        Vector3[] zoneCorners = new Vector3[4];
        boundsRect.GetWorldCorners(zoneCorners);

        float itemHalfW = rectTransform.rect.width * rectTransform.lossyScale.x / 2f;
        float itemHalfH = rectTransform.rect.height * rectTransform.lossyScale.y / 2f;

        float minX = zoneCorners[0].x + itemHalfW + paddingLeft;
        float maxX = zoneCorners[2].x - itemHalfW - paddingRight;
        float minY = zoneCorners[0].y + itemHalfH + paddingBottom;
        float maxY = zoneCorners[1].y - itemHalfH - paddingTop;

        Vector3 pos = rectTransform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        rectTransform.position = pos;
    }

    private void PickRandomDirection()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        moveDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        currentSpeed = Random.Range(minSpeed, maxSpeed);
        directionTimer = directionChangeInterval + Random.Range(-0.5f, 0.5f);
    }
}