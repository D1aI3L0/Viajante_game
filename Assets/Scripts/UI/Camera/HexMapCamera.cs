using UnityEngine;

public class HexMapCamera : MonoBehaviour
{
    private Transform swivel, stick;
    private float zoom = 1f;
    public float stickMinZoom, stickMaxZoom;
    public float swivelMinZoom, swivelMaxZoom;
    public float moveSpeedMinZoom, moveSpeedMaxZoom;
    public float rotationSpeed;
    private float rotationAngle;

    public HexGrid grid;
    public static HexMapCamera Instance;

    public static bool Locked
    {
        set
        {
            Instance.enabled = !value;
        }
    }

    private void Awake()
    {
        Instance = this;
        swivel = transform.GetChild(0);
        stick = swivel.GetChild(0);
    }

    private void OnEnable()
    {
        Instance = this;
        ValidatePosition();
    }

    private void Update()
    {
        float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
        if (zoomDelta != 0f)
        {
            AdjustZoom(zoomDelta);
        }

        float rotationDelta = Input.GetAxis("Rotation");
        if (rotationDelta != 0f)
        {
            AdjustRotation(rotationDelta);
        }

        float xDelta = Input.GetAxis("Horizontal");
        float zDelta = Input.GetAxis("Vertical");
        if (xDelta != 0f || zDelta != 0f)
        {
            AdjustPosition(xDelta, zDelta);
        }
    }

    private void AdjustZoom(float delta)
    {
        zoom = Mathf.Clamp01(zoom + delta);

        float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
        stick.localPosition = new Vector3(0f, 0f, distance);

        float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, zoom);
        swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
    }

    private void AdjustPosition(float xDelta, float zDelta)
    {
        Vector3 direction = transform.localRotation * new Vector3(xDelta, 0f, zDelta).normalized;
        float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
        float distance = Mathf.Lerp(moveSpeedMinZoom, moveSpeedMaxZoom, zoom) * damping * Time.deltaTime;

        Vector3 position = transform.localPosition;
        position += direction * distance;
        transform.localPosition = (grid.xWrapping || grid.zWrapping) ? WrapPosition(position) : ClampPosition(position);
    }

    private void AdjustRotation(float delta)
    {
        rotationAngle += delta * rotationSpeed * Time.deltaTime;
        if (rotationAngle < 0f)
        {
            rotationAngle += 360f;
        }
        else if (rotationAngle >= 360f)
        {
            rotationAngle -= 360f;
        }
        transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);
    }

    private Vector3 ClampPosition(Vector3 position)
    {
        float xMax = (grid.cellCountX - 0.5f) * HexMetrics.innerDiameter;
        position.x = Mathf.Clamp(position.x, 0f, xMax);

        float zMax = (grid.cellCountZ - 1) * HexMetrics.outerDiametr;
        position.z = Mathf.Clamp(position.z, 0f, zMax);

        return position;
    }

    private Vector3 WrapPosition(Vector3 position)
    {
        if (grid.xWrapping)
        {
            float width = grid.cellCountX * HexMetrics.innerDiameter;
            while (position.x < 0f)
            {
                position.x += width;
            }
            while (position.x > width)
            {
                position.x -= width;
            }
            
            grid.CenterMapX(position.x);
        }
        else
        {
            float xMax = (grid.cellCountX - 0.5f) * HexMetrics.innerDiameter;
            position.x = Mathf.Clamp(position.x, 0f, xMax);
        }

        if (grid.zWrapping)
        {
            float height = grid.cellCountZ * HexMetrics.outerDiametr;
            while (position.z < 0f)
            {
                position.z += height;
            }
            while (position.z > height)
            {
                position.z -= height;
            }

            grid.CenterMapZ(position.z);
        }
        else
        {
            float zMax = (grid.cellCountZ - 1) * (1.5f * HexMetrics.outerRadius);
            position.z = Mathf.Clamp(position.z, 0f, zMax);
        }
        return position;
    }

    public static void ValidatePosition()
    {
        Instance.AdjustPosition(0f, 0f);
    }

    public static void MoveToBase()
    {
        Instance.AdjustPosition(Base.Instance.transform.position.x, Base.Instance.transform.position.z);
    }
}
