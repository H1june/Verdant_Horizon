using UnityEngine;

public class HomingMissile : MonoBehaviour
{
    public float speed = 10f; // �ʱ� �ӵ�
    public float acceleration = 30f; // ���ӵ�
    public float rotationSpeed = 200f; // ȸ�� �ӵ�
    public float detectionRadius = 200f; // Ž�� �ݰ�
    public float initialFlightTime = 0.3f; // �ʱ� ���� ���� �ð�

    private Rigidbody2D rb;
    private Transform target;
    private Vector2 initialDirection; // �ʱ� ���� 
    private float initialFlightTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        FindClosestTarget(); // �ʱ� Ÿ�� ����
        if (target != null)
        {
            // Ÿ���� ������ ��� �ʱ� ������ ����
            initialDirection = (target.position - transform.position).normalized;
        }
        else
        {
            // Ÿ���� ������ ���� �������� ����
            initialDirection = transform.up;
        }
        initialFlightTimer = initialFlightTime; // �ʱ� ���� ���� �ð� ����
    }

    void FixedUpdate()
    {
        if (target == null)
        {
            FindClosestTarget(); // Ÿ���� ���� ��� ���� Ž��
            return;
        }

        // �ʱ� ���� ���� �ܰ�
        if (initialFlightTimer > 0)
        {
            initialFlightTimer -= Time.fixedDeltaTime;

            // �ʱ� �������� ������ �̵�
            rb.velocity = initialDirection * speed;
            return; // ���� ����� �ϴ� ��Ȱ��ȭ
        }

        // ���� Ȱ��ȭ (��ǥ�� ����)
        Vector2 direction = (target.position - transform.position).normalized;

        // ��ǥ�� �������� �ε巴�� ȸ��
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; // 2D ȸ�� ���� ����
        float currentAngle = Mathf.LerpAngle(rb.rotation, targetAngle, Time.fixedDeltaTime * rotationSpeed);

        rb.rotation = currentAngle; // Rigidbody2D�� ȸ�� ����

        // transform.up ������Ʈ: Rigidbody2D�� ȸ�� ������ ���� �̵�
        Vector2 forward = new Vector2(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad));

        // �̻��� �̵�
        speed += acceleration * Time.fixedDeltaTime;
        rb.velocity = forward * speed;
    }

    // �浹 ó��
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster"))
        {
            Destroy(gameObject); // �̻��� �ı�
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

        target = closestEnemy; // ���� ����� ���� Ÿ������ ����
    }
}
