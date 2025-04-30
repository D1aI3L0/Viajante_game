using UnityEngine;

public class BattleMapCamera : MonoBehaviour
{
    Transform swivel, stick;
    float zoom = 1f;
    
    [Header("Параметры зума камеры")]
    [Tooltip("Минимальное расстояние между Stick и камерой (при максимальном приближении).")]
    public float stickMinZoom = 5f;
    
    [Tooltip("Максимальное расстояние между Stick и камерой (при максимальном отдалении).")]
    public float stickMaxZoom = 15f;
    
    [Tooltip("Минимальный угол наклона Swivel (при максимальном приближении).")]
    public float swivelMinAngle = 30f;
    
    [Tooltip("Максимальный угол наклона Swivel (при максимальном отдалении).")]
    public float swivelMaxAngle = 70f;
    
    [Header("Параметры перемещения камеры")]
    [Tooltip("Минимальная скорость перемещения камеры (при приближении).")]
    public float moveSpeedMinZoom = 10f;
    
    [Tooltip("Максимальная скорость перемещения камеры (при отдалении).")]
    public float moveSpeedMaxZoom = 20f;
    
    [Tooltip("Скорость вращения камеры (в градусах в секунду).")]
    public float rotationSpeed = 90f;
    float rotationAngle;
    
    [Header("Настройки боевой карты")]
    [Tooltip("Ссылка на BattleConfig, содержащий параметры боевой карты.")]
    public BattleConfig battleConfig;
    
    [Tooltip("Ссылка на BattleMapManager, где объявлены константы геометрии ячеек (например, innerDiameter и outerRadius).")]
    public BattleMapManager battleMapManager;
    
    // Настройка скорости зума по клавиатуре
    [Header("Альтернативное управление зумом")]
    [Tooltip("Скорость изменения зума при нажатии клавиш + и -.")]
    public float keyboardZoomSpeed = 1f;
    
    // Границы перемещения камеры в мировых координатах:
    Vector3 minCameraPosition;
    Vector3 maxCameraPosition;
    
    void Awake()
    {
        // Иерархия: BattleMapCamera -> Swivel -> Stick -> Main Camera.
        swivel = transform.GetChild(0);
        stick = swivel.GetChild(0);
    }

    void Start()
    {
        CalculateCameraBounds();
    }

    void Update()
    {
        // Обрабатываем значение зума с мышиного колёсика:
        float zoomDelta = Input.GetAxis("Mouse ScrollWheel");

        // Обрабатываем альтернативное управление зумом клавиатурой:
        if (Input.GetKey(KeyCode.KeypadPlus) || Input.GetKey(KeyCode.Equals))
        {
            zoomDelta -= keyboardZoomSpeed * Time.deltaTime; // Плавное увеличение зума
        }
        if (Input.GetKey(KeyCode.KeypadMinus) || Input.GetKey(KeyCode.Minus))
        {
            zoomDelta += keyboardZoomSpeed * Time.deltaTime; // Плавное уменьшение зума
        }

        if (Mathf.Abs(zoomDelta) > 0f)
        {
            AdjustZoom(zoomDelta);
        }

        // Обработка вращения камеры
        float rotationDelta = Input.GetAxis("Rotation");
        if (Mathf.Abs(rotationDelta) > 0f)
        {
            AdjustRotation(rotationDelta);
        }

        // Обработка перемещения камеры по горизонтали и вертикали
        float xDelta = Input.GetAxis("Horizontal");
        float zDelta = Input.GetAxis("Vertical");
        if (Mathf.Abs(xDelta) > 0f || Mathf.Abs(zDelta) > 0f)
        {
            AdjustPosition(xDelta, zDelta);
        }
    }
    
    void AdjustZoom(float delta)
    {
        zoom = Mathf.Clamp01(zoom + delta);
        
        // Интерполируем расстояние от Stick с учетом зума.
        // Поскольку исходная позиция Stick имеет отрицательное значение по оси Z (например, (0,0,-45)),
        // устанавливаем его позицию как -distance.
        float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
        stick.localPosition = new Vector3(0f, 0f, -distance);
        
        // Интерполируем угол наклона Swivel.
        float angle = Mathf.Lerp(swivelMinAngle, swivelMaxAngle, zoom);
        swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
    }
    
    void AdjustPosition(float xDelta, float zDelta)
    {
        Vector3 direction = transform.localRotation * new Vector3(xDelta, 0f, zDelta).normalized;
        float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
        float moveSpeed = Mathf.Lerp(moveSpeedMinZoom, moveSpeedMaxZoom, zoom);
        float distance = moveSpeed * damping * Time.deltaTime;
        Vector3 position = transform.localPosition;
        position += direction * distance;
        transform.localPosition = ClampPosition(position);
    }
    
    void AdjustRotation(float delta)
    {
        rotationAngle += delta * rotationSpeed * Time.deltaTime;
        rotationAngle %= 360f;
        transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);
    }
    
    // Вычисляем границы камеры на основе данных из BattleConfig и констант из BattleMapManager.
    void CalculateCameraBounds()
    {
        // Используем публичные константы из BattleMapManager.
        float innerDiameter = BattleMapManager.innerDiameter;
        float outerRadius = BattleMapManager.outerRadius;
        
        // Вычисляем ширину карты в мировых координатах:
        float mapWidth = battleConfig.battleMapSize.x * innerDiameter;
        
        // Для шестиугольных ячеек вертикальный шаг равен outerRadius * 1.5, первая строка – outerRadius.
        float mapHeight = outerRadius + (battleConfig.battleMapSize.y - 1) * (outerRadius * 1.5f);
        
        minCameraPosition = Vector3.zero;
        maxCameraPosition = new Vector3(mapWidth, 0f, mapHeight);
    }
    
    // Ограничиваем позицию камеры так, чтобы она не выходила за пределы карты.
    Vector3 ClampPosition(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, minCameraPosition.x, maxCameraPosition.x);
        position.z = Mathf.Clamp(position.z, minCameraPosition.z, maxCameraPosition.z);
        return position;
    }
}
