using UnityEngine;

public class HomingMissile : MonoBehaviour
{
    public float speed = 10f; // 초기 속도
    public float acceleration = 30f; // 가속도
    public float rotationSpeed = 200f; // 회전 속도
    public float detectionRadius = 200f; // 탐색 반경
    public float initialFlightTime = 0.3f; // 초기 직진 비행 시간

    private Rigidbody2D rb;
    private Transform target;
    private Vector2 initialDirection; // 초기 방향 
    private float initialFlightTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        FindClosestTarget(); // 초기 타겟 설정
        if (target != null)
        {
            // 타겟이 존재할 경우 초기 방향을 설정
            initialDirection = (target.position - transform.position).normalized;
        }
        else
        {
            // 타겟이 없으면 현재 방향으로 유지
            initialDirection = transform.up;
        }
        initialFlightTimer = initialFlightTime; // 초기 직진 비행 시간 설정
    }

    void FixedUpdate()
    {
        if (target == null)
        {
            FindClosestTarget(); // 타겟이 없는 경우 새로 탐색
            return;
        }

        // 초기 직진 비행 단계
        if (initialFlightTimer > 0)
        {
            initialFlightTimer -= Time.fixedDeltaTime;

            // 초기 방향으로 일직선 이동
            rb.velocity = initialDirection * speed;
            return; // 유도 기능은 일단 비활성화
        }

        // 유도 활성화 (목표물 추적)
        Vector2 direction = (target.position - transform.position).normalized;

        // 목표물 방향으로 부드럽게 회전
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; // 2D 회전 보정 없음
        float currentAngle = Mathf.LerpAngle(rb.rotation, targetAngle, Time.fixedDeltaTime * rotationSpeed);

        rb.rotation = currentAngle; // Rigidbody2D의 회전 설정

        // transform.up 업데이트: Rigidbody2D의 회전 각도에 따라 이동
        Vector2 forward = new Vector2(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad));

        // 미사일 이동
        speed += acceleration * Time.fixedDeltaTime;
        rb.velocity = forward * speed;
    }

    // 충돌 처리
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster"))
        {
            Destroy(gameObject); // 미사일 파괴
        }
    }

    void FindClosestTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Monster");
        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance && distance <= detectionRadius)
            {
                closestDistance = distance;
                closestEnemy = enemy.transform;
            }
        }

        target = closestEnemy; // 가장 가까운 적을 타겟으로 설정
    }
}
