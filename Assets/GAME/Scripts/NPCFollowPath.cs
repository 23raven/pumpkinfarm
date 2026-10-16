using UnityEngine;
using System.Collections.Generic;

public class NPCFollowPath2D : MonoBehaviour
{
    [Header("Компоненты")]
    [SerializeField] private SpriteRenderer spriteRenderer; // Ссылка на спрайт непися

    [Header("Настройки движения")]
    [SerializeField] private List<Transform> waypoints; // Список точек маршрута
    [SerializeField] private float speed = 3f;          // Скорость движения NPC
    [SerializeField] private float arrivalDistance = 0.2f; // Дистанция до точки для прибытия

    [Header("Остановки на точках")]
    [SerializeField] private float waitTime = 1.5f;     // Сколько секунд стоять на точке

    [Header("Тип маршрута")]
    [SerializeField] private bool loopPath = true;      // true - по кругу, false - туда-обратно

    private int currentWaypointIndex = 0;
    private bool movingForward = true;
    private float waitTimer = 0f;
    private bool isWaiting = false;

    private void Start()
    {
        // Если забыли перетащить SpriteRenderer, скрипт попытается найти его сам
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (waypoints == null || waypoints.Count == 0)
        {
            Debug.LogWarning($"У NPC {gameObject.name} не заданы точки маршрута!");
            enabled = false;
            return;
        }

        // Ставим NPC в начальную точку в 2D пространстве
        transform.position = new Vector3(waypoints[0].position.x, waypoints[0].position.y, transform.position.z);
    }

    private void Update()
    {
        if (isWaiting)
        {
            HandleWaiting();
        }
        else
        {
            MoveAndFlip();
        }
    }

    private void MoveAndFlip()
    {
        Transform targetWaypoint = waypoints[currentWaypointIndex];

        // 1. ДВИЖЕНИЕ в 2D (по осям X и Y, Z оставляем нетронутым)
        Vector3 targetPosition = new Vector3(targetWaypoint.position.x, targetWaypoint.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // 2. ПОВОРОТ СПРАЙТА (влево / вправо по оси X)
        float directionX = targetWaypoint.position.x - transform.position.x;

        if (directionX > 0.01f)
        {
            // Идет вправо -> спрайт не отзеркален (или наоборот, зависит от исходного рисунка)
            spriteRenderer.flipX = false;

            // АЛЬТЕРНАТИВА (если нужно развернуть весь объект со всеми дочерними элементами):
            // transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (directionX < -0.01f)
        {
            // Идет влево -> отзеркаливаем спрайт
            spriteRenderer.flipX = true;

            // transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        // 3. ПРОВЕРКА ПРИБЫТИЯ
        // Считаем дистанцию только по X и Y
        float distance = Vector2.Distance(transform.position, targetWaypoint.position);
        if (distance < arrivalDistance)
        {
            if (waitTime > 0f)
            {
                isWaiting = true;
                waitTimer = waitTime;
            }
            else
            {
                SelectNextWaypoint();
            }
        }
    }

    private void HandleWaiting()
    {
        waitTimer -= Time.deltaTime;
        if (waitTimer <= 0f)
        {
            isWaiting = false;
            SelectNextWaypoint();
        }
    }

    private void SelectNextWaypoint()
    {
        if (loopPath)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
        }
        else
        {
            if (movingForward)
            {
                currentWaypointIndex++;
                if (currentWaypointIndex >= waypoints.Count)
                {
                    currentWaypointIndex = waypoints.Count - 2;
                    movingForward = false;
                }
            }
            else
            {
                currentWaypointIndex--;
                if (currentWaypointIndex < 0)
                {
                    currentWaypointIndex = 1;
                    movingForward = true;
                }
            }
        }
    } 

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count < 2) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] == null) continue;

            Gizmos.DrawWireSphere(waypoints[i].position, 0.2f);

            if (i < waypoints.Count - 1 && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
            else if (loopPath && waypoints != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[0].position);
            }
        }
    }
}
