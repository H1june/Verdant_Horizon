using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShortcutManagement;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Transform target; // Ÿ�� ����
    public float attackDelay = 2.0f; // ���� ������
    public float patrolSpeed = 2.0f; // ��Ʈ�� �̵� �ӵ�
    public float patrolRange = 3.0f; // ��ȸ ���� ����
    public float moveSpeed = 2.0f; // �̵� �ӵ�
    public bool isMovingLeft = true; // ó���� �������� ����
    public bool isGround = false; // �ٴ� üũ ����
    public bool isChasing = false;
    public Transform[] patrolPoints; // ��Ʈ�� ����Ʈ �迭
    private int currentPatrolIndex; // ���� ��Ʈ�� ����Ʈ �ε���

    private Vector3 startPosition; // ���� ��ġ ����
    private bool isAttacking = false; // ���� ������ Ȯ���ϴ� �÷���
    private Enemy enemy; // Enemy ��ũ��Ʈ ����
    private Animator enemyAnimator; // �ִϸ����� ����
    private CapsuleCollider2D capCol2D; // �ٴ� üũ��
    private BoxCollider2D boxCol2D; // �÷��� üũ��
    public float safeDistance;

    public GameObject AttackEffectPrefab; // ���ͺ� ���� ������

    public float prevDealy;     
    void Start()
    {
        startPosition = transform.position; // ���� ���� ����
        enemy = GetComponent<Enemy>();
        enemyAnimator = enemy.enemyAnimator;
        capCol2D = GetComponent<CapsuleCollider2D>();
        currentPatrolIndex = 0; // �ʱ� ��Ʈ�� ����Ʈ ����
        LoadEffectPrefab();
    }

    void Update()
    {
        FaceTarget();

        // ���� ������ ó��
        if (attackDelay > 0)
        {
            attackDelay -= Time.deltaTime;
            if (attackDelay <= 0)
            {
                attackDelay = 0;
                isAttacking = false;
                enemyAnimator.SetBool("attack", false); // ���� ����
            }
        }

        // ���� ���̶�� �ٸ� ����(����, ��Ʈ��)�� ���� ����
        if (isAttacking)
        {
            return; // ���� ���� ���� �̵� �� �ٸ� ���� ����
        }

        // �ٴ� üũ
        CheckGround();

        // �÷��̾ ���� ������ �ִ��� Ȯ��
        if (IsTargetInFieldOfVision())
        {
            ChaseTarget(); // �÷��̾� ����
        }
        else
        {
            Patrol(); // ��Ʈ�� ����
        }

        // ���� �Ÿ� üũ
        float distance = Vector3.Distance(transform.position, target.position);
        if (!isAttacking && attackDelay == 0 && distance <= enemy.atkRange)
        {
            FaceTarget();
            AttackTarget();
        }

        // �ִϸ��̼� ���¿� ���� attack ���� ������Ʈ
        if (isAttacking && enemyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            if (enemyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
            {
                enemyAnimator.SetBool("attack", false); // ���� ���� ����
                isAttacking = false; // ���� ���� ��Ȱ��ȭ
            }
        }
    }


    public void Monster_AttackTrue()
    {
        isAttacking = true;
    }

    public void Monster_AttackFalse()
    {
        isAttacking = false;
    }

    void Patrol()
    {
        Elf elf = target.GetComponent<Elf>();

        if (patrolPoints.Length == 0) return; // ��Ʈ�� ����Ʈ�� ���� ������ �׾��ٸ� ����

        // ���� ��ǥ ��Ʈ�� ����Ʈ
        Transform targetPatrolPoint = patrolPoints[currentPatrolIndex];

        // ��ǥ ��Ʈ�� ����Ʈ�� �̵�
        transform.position = Vector2.MoveTowards(transform.position, targetPatrolPoint.position, patrolSpeed * Time.deltaTime);

        // ��ǥ ��Ʈ�� ����Ʈ�� �����ϸ� ���� ����Ʈ�� ����
        if (Vector2.Distance(transform.position, targetPatrolPoint.position) < 0.1f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length; // ���� ��Ʈ�� ����Ʈ�� �̵�
        }

        // �̵��ϴ� ���⿡ ���� localScale.flip ����
        Vector3 direction = targetPatrolPoint.position - transform.position; // �̵� ���� ���

        if (direction.x > 0 && transform.localScale.x > 0) // ���������� �̵� ���̸�
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z); // x ���� ������ �����Ͽ� �������� ��
        }
        else if (direction.x < 0 && transform.localScale.x < 0) // �������� �̵� ���̸�
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z); // x ���� ����� �����Ͽ� ������ ��
        }

        // �̵� �ִϸ��̼� ����
        enemyAnimator.SetBool("moving", true);
    }

    // �÷��̾ ���� ���� �ȿ� �ִ��� Ȯ���ϴ� �Լ�
    bool IsTargetInFieldOfVision()
    {
        float distance = Vector3.Distance(transform.position, target.position);
        return distance <= enemy.fieldOfVision; // target�� ���� ������ �ִ��� ���� ��ȯ
    }

    void AttackTarget()
    {
        Elf elf = target.GetComponent<Elf>();

        // ������ ���� �ʾҰ�, �ǰ� ���°� �ƴ� ���� ����
        if (!elf.isDead && elf != null && !elf.isHit)
        {
            // ���� ���� �ƴϰ�, ���� ��� �ð��� 0 ������ ���� ����
            if (!isAttacking && attackDelay <= 0)
            {
                // ���� �ִϸ��̼� Ʈ���� ����
                enemyAnimator.SetTrigger("attack");
                if (enemy.enemyName == "Eagle" || enemy.enemyName == "Bomber") 
                {
                    SpawnAttackEffect();
                }
                else if (enemy.enemyName == "Assassin")
                {
                    AssassinAttack();
                }
                // ���� ��� �ð� ����
                attackDelay = enemy.atkSpeed;
                // ���� ���� Ȱ��ȭ
                Monster_AttackTrue();

                // ������ ó���� �ִϸ��̼� �̺�Ʈ�� ���� �̷����
            }
            // ���� ���� ���� ó��
            Monster_AttackFalse();
        }

        // ���� ���� �� ���� ������ ����
        if (isAttacking)
        {
            attackDelay -= Time.deltaTime;

            // ���� �ִϸ��̼� ���¸� Ȯ��
            AnimatorStateInfo stateInfo = enemyAnimator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("dron_attack")) // "dron_attack"�� ���� ���� �ִϸ��̼� �̸����� �����ϼ���.
            {
                if (stateInfo.normalizedTime >= 1.0f) // �ִϸ��̼� ���� ���� Ȯ��
                {
                    enemyAnimator.SetBool("isAttacked", false);
                    isAttacking = false;
                }
            }
        }
    }

    public void DealDamageToTarget()
    {
        Elf elf = target.GetComponent<Elf>();
        if (elf != null && !elf.isDead && !elf.isInvincible)
        {
            elf.TakeDamage(enemy.atkDmg, transform);
        }
    }
    void AssassinAttack()
    {
        // �ڷ�ƾ ����
        StartCoroutine(PerformAssassinAttack());
    }

    private IEnumerator PerformAssassinAttack()
    {
        // 1. �÷��̾��� ���� Ȯ�� (����: -1, ������: 1)
        float playerDirection = Mathf.Sign(target.localScale.x);

        // 2. �÷��̾��� ���� ��ġ ���
        Vector3 playerBackPosition = target.position - (Vector3.right * playerDirection * 1.5f); // �ڷ� 1.5 ���� �Ÿ� �̵�
        Vector3 initialPosition = transform.position; // ���� ���� ��ġ ����

        // 3. �ε巴�� �̵�
        float moveDuration = 0.2f; // �̵� �ð�
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            transform.position = Vector3.Lerp(initialPosition, playerBackPosition, elapsedTime / moveDuration);
            yield return null; // ���� �����ӱ��� ���
        }
        transform.position = playerBackPosition; // ���� ��ġ�� ��Ȯ�� ����

        Monster_AttackFalse();
    }


    void SpawnAttackEffect()
    {
        GameObject effectPrefab = AttackEffectPrefab;

        if (effectPrefab != null)
        {
            // �⺻ ����Ʈ ���� ��ġ ���
            Vector3 spawnPosition = transform.position + new Vector3(transform.localScale.x * -1.5f, 0, 0);

            // �⺻ ����Ʈ ����
            GameObject effect = Instantiate(effectPrefab, spawnPosition, Quaternion.identity);

            // ����Ʈ�� ���� ���� (�÷��̾ ������ ���� ����Ʈ�� ������)
            Vector3 effectScale = effect.transform.localScale;
            effectScale.x = Mathf.Abs(effectScale.x) * (transform.localScale.x > 0 ? 1 : -1);

            effect.transform.localScale = effectScale;

            // Bomber�� ��� Ư�� �ִϸ��̼� ����
            if (enemy.enemyName == "Bomber")
            {
                StartCoroutine(BomberEffectBounce(effect));
            }
        }
    }

    // �÷��̾ �����ϴ� �Լ�
    void ChaseTarget()
    {
        isChasing = true;
        float distanceX = Mathf.Abs(target.position.x - transform.position.x);

        // X�� ���̰� ������ �����Ÿ����� ũ�� �̵�
        if (distanceX > safeDistance)
        {
            float directionX = target.position.x - transform.position.x;
            directionX = Mathf.Sign(directionX); // X�� ���� ���� (-1: ����, 1: ������)

            // ���ο� ��ġ ���
            Vector3 newPosition = transform.position;
            newPosition.x += directionX * moveSpeed * Time.deltaTime;

            // Y�� ���� (�⺻������ Y��ǥ ����)
            newPosition.y = startPosition.y;

            // ���� ��ġ ����
            transform.position = newPosition;

            // �ִϸ��̼� ����
            enemyAnimator.SetBool("moving", true);
        }
        else
        {
            // �ʹ� ������ �̵��� ���߰� �ִϸ��̼� ����
            enemyAnimator.SetBool("moving", false);
        }
    }

    void FaceTarget()
    {
        // �÷��̾���� X�� ���̰� �ʹ� ������ ������ �������� ���� (��: 0.5f)
        if (Mathf.Abs(target.position.x - transform.position.x) > 0.5f)
        {
            if (target.position.x < transform.position.x) // Ÿ���� ���ʿ� ���� ��
            {
                transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z); // �������� �ٶ󺸰� ����
            }
            else if (target.position.x > transform.position.x)
            {
                transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z); // ���������� �ٶ󺸰� ����
            }
        }
    }

    void CheckGround()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(capCol2D.bounds.center, capCol2D.bounds.size, 0f, Vector2.down, 0.02f, LayerMask.GetMask("Ground"));
        isGround = raycastHit.collider != null; // �ٴ��� �ִ��� ���� ����
    }

    bool CanMoveForward()
    {
        float dir = transform.localScale.x > 0 ? 1 : -1; // �ٶ󺸴� ���⿡ ���� ���� ���� ����
        Vector2 rayDirection = new Vector2(dir, 0);

        Vector2 rayStart = (Vector2)transform.position + new Vector2(dir * 0.5f, 0.2f); // �ణ ������ �߻�

        // ����ĳ��Ʈ�� �߻��Ͽ� ���� �ִ��� Ȯ��
        RaycastHit2D hit = Physics2D.Raycast(rayStart, rayDirection, 3.0f, LayerMask.GetMask("Ground")); // Wall ���̾�� ����ĳ��Ʈ
        if (hit)
        {
            Debug.Log("�� �¾Ҵ�!!!");
        }
        Debug.DrawRay(rayStart, rayDirection * 0.5f, Color.red); // ����׿� ���� ǥ��

        return hit.collider == null; // ���� ������ �̵� ����
    }
    void LoadEffectPrefab()
    {
        if (enemy.enemyName == "Eagle")
        {
            // PunchffectPrefab�� Resources �������� �ε�
            AttackEffectPrefab = Resources.Load<GameObject>("Enemy/Eagle/Eagle_Tornado");
            if (AttackEffectPrefab == null) Debug.LogError("AttackEffect �������� ã�� �� �����ϴ�! ��θ� Ȯ���ϼ���.");
            else if (AttackEffectPrefab) Debug.Log("AttackEffect ������ �ε� �Ϸ�.");
        }

        // else if (enemy.enemyName == "Bomber")
        // {
        //     // Bomber Attack Effect�� Resources �������� �ε�
        //     AttackEffectPrefab = Resources.Load<GameObject>("Enemy/Bomber_Bomb");
        //     if (AttackEffectPrefab == null) Debug.LogError("AttackEffect �������� ã�� �� �����ϴ�! ��θ� Ȯ���ϼ���.");
        //     else if (AttackEffectPrefab) Debug.Log("AttackEffect ������ �ε� �Ϸ�.");
        // }
    }

    private IEnumerator BomberEffectBounce(GameObject effect)
    {
        
        float bounceHeight = 0.5f; // Ƣ�� �ְ� ����
        float bounceDuration = 0.3f; // �� �� Ƣ�� �� �ɸ��� �ð�
        int bounceCount = 3; // Ƣ�� Ƚ��
        float moveDistance = 0.5f; // �������� �̵��� �Ÿ�

        // �̵� ���� ��� (�÷��̾ �������� ���� �ִٸ� ���, �����̶�� ����)
        float moveDirection = transform.localScale.x > 0 ? -1 : 1;

        Vector3 initialPosition = effect.transform.position + new Vector3(-0.5f, -0.5f, 0);

        for (int i = 0; i < bounceCount; i++)
        {
            // ��ǥ ��ġ (������ �̵��� ����)
            Vector3 targetPosition = initialPosition + new Vector3(moveDirection * moveDistance, 0, 0); // Y�� ���� ���� ����
            yield return ParabolicMoveEffect(effect, initialPosition, targetPosition, bounceHeight, bounceDuration);

            // �̵� ������ ����
            initialPosition = targetPosition;
        }

        // ������ Ƣ�� �� ����
        Destroy(effect);
    }

    private IEnumerator ParabolicMoveEffect(GameObject effect, Vector3 startPosition, Vector3 targetPosition, float height, float duration)
    {
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // ���� ��ġ: ���� ����
            float x = Mathf.Lerp(startPosition.x, targetPosition.x, t);

            // ���� ��ġ: ������ ����
            float y = Mathf.Lerp(startPosition.y, targetPosition.y, t) + height * Mathf.Sin(Mathf.PI * t);

            // �� ��ġ ����
            effect.transform.position = new Vector3(x, y, startPosition.z);
            yield return null;
        }

        // ������ ��ġ �����ϰ� ����
        effect.transform.position = targetPosition;
    }
}