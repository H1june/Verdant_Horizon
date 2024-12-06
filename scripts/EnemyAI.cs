using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShortcutManagement;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Transform target; // 타겟 엘프
    public float attackDelay = 2.0f; // 공격 딜레이
    public float patrolSpeed = 2.0f; // 패트롤 이동 속도
    public float patrolRange = 3.0f; // 배회 범위 설정
    public float moveSpeed = 2.0f; // 이동 속도
    public bool isMovingLeft = true; // 처음에 왼쪽으로 시작
    public bool isGround = false; // 바닥 체크 여부
    public bool isChasing = false;
    public Transform[] patrolPoints; // 패트롤 포인트 배열
    private int currentPatrolIndex; // 현재 패트롤 포인트 인덱스

    private Vector3 startPosition; // 시작 위치 저장
    private bool isAttacking = false; // 공격 중인지 확인하는 플래그
    private Enemy enemy; // Enemy 스크립트 참조
    private Animator enemyAnimator; // 애니메이터 참조
    private CapsuleCollider2D capCol2D; // 바닥 체크용
    private BoxCollider2D boxCol2D; // 플랫폼 체크용
    public float safeDistance;

    public GameObject AttackEffectPrefab; // 몬스터별 공격 프리팹

    public float prevDealy;     
    void Start()
    {
        startPosition = transform.position; // 시작 지점 저장
        enemy = GetComponent<Enemy>();
        enemyAnimator = enemy.enemyAnimator;
        capCol2D = GetComponent<CapsuleCollider2D>();
        currentPatrolIndex = 0; // 초기 패트롤 포인트 설정
        LoadEffectPrefab();
    }

    void Update()
    {
        FaceTarget();

        // 공격 딜레이 처리
        if (attackDelay > 0)
        {
            attackDelay -= Time.deltaTime;
            if (attackDelay <= 0)
            {
                attackDelay = 0;
                isAttacking = false;
                enemyAnimator.SetBool("attack", false); // 공격 종료
            }
        }

        // 공격 중이라면 다른 동작(추적, 패트롤)을 하지 않음
        if (isAttacking)
        {
            return; // 공격 중일 때는 이동 및 다른 동작 중지
        }

        // 바닥 체크
        CheckGround();

        // 플레이어가 인지 범위에 있는지 확인
        if (IsTargetInFieldOfVision())
        {
            ChaseTarget(); // 플레이어 추적
        }
        else
        {
            Patrol(); // 패트롤 동작
        }

        // 공격 거리 체크
        float distance = Vector3.Distance(transform.position, target.position);
        if (!isAttacking && attackDelay == 0 && distance <= enemy.atkRange)
        {
            FaceTarget();
            AttackTarget();
        }

        // 애니메이션 상태에 따라 attack 상태 업데이트
        if (isAttacking && enemyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            if (enemyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
            {
                enemyAnimator.SetBool("attack", false); // 공격 상태 해제
                isAttacking = false; // 공격 상태 비활성화
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

        if (patrolPoints.Length == 0) return; // 패트롤 포인트가 없고 엘프가 죽었다면 종료

        // 현재 목표 패트롤 포인트
        Transform targetPatrolPoint = patrolPoints[currentPatrolIndex];

        // 목표 패트롤 포인트로 이동
        transform.position = Vector2.MoveTowards(transform.position, targetPatrolPoint.position, patrolSpeed * Time.deltaTime);

        // 목표 패트롤 포인트에 도달하면 다음 포인트로 변경
        if (Vector2.Distance(transform.position, targetPatrolPoint.position) < 0.1f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length; // 다음 패트롤 포인트로 이동
        }

        // 이동하는 방향에 따른 localScale.flip 설정
        Vector3 direction = targetPatrolPoint.position - transform.position; // 이동 방향 계산

        if (direction.x > 0 && transform.localScale.x > 0) // 오른쪽으로 이동 중이면
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z); // x 값을 음수로 설정하여 오른쪽을 봄
        }
        else if (direction.x < 0 && transform.localScale.x < 0) // 왼쪽으로 이동 중이면
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z); // x 값을 양수로 설정하여 왼쪽을 봄
        }

        // 이동 애니메이션 설정
        enemyAnimator.SetBool("moving", true);
    }

    // 플레이어가 인지 범위 안에 있는지 확인하는 함수
    bool IsTargetInFieldOfVision()
    {
        float distance = Vector3.Distance(transform.position, target.position);
        return distance <= enemy.fieldOfVision; // target이 인지 범위에 있는지 여부 반환
    }

    void AttackTarget()
    {
        Elf elf = target.GetComponent<Elf>();

        // 엘프가 죽지 않았고, 피격 상태가 아닐 때만 공격
        if (!elf.isDead && elf != null && !elf.isHit)
        {
            // 공격 중이 아니고, 공격 대기 시간이 0 이하일 때만 공격
            if (!isAttacking && attackDelay <= 0)
            {
                // 공격 애니메이션 트리거 설정
                enemyAnimator.SetTrigger("attack");
                if (enemy.enemyName == "Eagle" || enemy.enemyName == "Bomber") 
                {
                    SpawnAttackEffect();
                }
                else if (enemy.enemyName == "Assassin")
                {
                    AssassinAttack();
                }
                // 공격 대기 시간 설정
                attackDelay = enemy.atkSpeed;
                // 공격 상태 활성화
                Monster_AttackTrue();

                // 데미지 처리는 애니메이션 이벤트를 통해 이루어짐
            }
            // 공격 상태 종료 처리
            Monster_AttackFalse();
        }

        // 공격 중일 때 공격 딜레이 감소
        if (isAttacking)
        {
            attackDelay -= Time.deltaTime;

            // 공격 애니메이션 상태를 확인
            AnimatorStateInfo stateInfo = enemyAnimator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("dron_attack")) // "dron_attack"을 실제 공격 애니메이션 이름으로 변경하세요.
            {
                if (stateInfo.normalizedTime >= 1.0f) // 애니메이션 종료 시점 확인
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
        // 코루틴 시작
        StartCoroutine(PerformAssassinAttack());
    }

    private IEnumerator PerformAssassinAttack()
    {
        // 1. 플레이어의 방향 확인 (왼쪽: -1, 오른쪽: 1)
        float playerDirection = Mathf.Sign(target.localScale.x);

        // 2. 플레이어의 뒤쪽 위치 계산
        Vector3 playerBackPosition = target.position - (Vector3.right * playerDirection * 1.5f); // 뒤로 1.5 단위 거리 이동
        Vector3 initialPosition = transform.position; // 적의 현재 위치 저장

        // 3. 부드럽게 이동
        float moveDuration = 0.2f; // 이동 시간
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            transform.position = Vector3.Lerp(initialPosition, playerBackPosition, elapsedTime / moveDuration);
            yield return null; // 다음 프레임까지 대기
        }
        transform.position = playerBackPosition; // 최종 위치를 정확히 설정

        Monster_AttackFalse();
    }


    void SpawnAttackEffect()
    {
        GameObject effectPrefab = AttackEffectPrefab;

        if (effectPrefab != null)
        {
            // 기본 이펙트 생성 위치 계산
            Vector3 spawnPosition = transform.position + new Vector3(transform.localScale.x * -1.5f, 0, 0);

            // 기본 이펙트 생성
            GameObject effect = Instantiate(effectPrefab, spawnPosition, Quaternion.identity);

            // 이펙트의 방향 설정 (플레이어가 왼쪽을 보면 이펙트를 뒤집음)
            Vector3 effectScale = effect.transform.localScale;
            effectScale.x = Mathf.Abs(effectScale.x) * (transform.localScale.x > 0 ? 1 : -1);

            effect.transform.localScale = effectScale;

            // Bomber의 경우 특수 애니메이션 적용
            if (enemy.enemyName == "Bomber")
            {
                StartCoroutine(BomberEffectBounce(effect));
            }
        }
    }

    // 플레이어를 추적하는 함수
    void ChaseTarget()
    {
        isChasing = true;
        float distanceX = Mathf.Abs(target.position.x - transform.position.x);

        // X축 차이가 설정된 안전거리보다 크면 이동
        if (distanceX > safeDistance)
        {
            float directionX = target.position.x - transform.position.x;
            directionX = Mathf.Sign(directionX); // X축 방향 설정 (-1: 왼쪽, 1: 오른쪽)

            // 새로운 위치 계산
            Vector3 newPosition = transform.position;
            newPosition.x += directionX * moveSpeed * Time.deltaTime;

            // Y축 고정 (기본적으로 Y좌표 유지)
            newPosition.y = startPosition.y;

            // 최종 위치 적용
            transform.position = newPosition;

            // 애니메이션 설정
            enemyAnimator.SetBool("moving", true);
        }
        else
        {
            // 너무 가까우면 이동을 멈추고 애니메이션 정지
            enemyAnimator.SetBool("moving", false);
        }
    }

    void FaceTarget()
    {
        // 플레이어와의 X축 차이가 너무 작으면 방향을 변경하지 않음 (예: 0.5f)
        if (Mathf.Abs(target.position.x - transform.position.x) > 0.5f)
        {
            if (target.position.x < transform.position.x) // 타겟이 왼쪽에 있을 때
            {
                transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z); // 왼쪽으로 바라보게 설정
            }
            else if (target.position.x > transform.position.x)
            {
                transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z); // 오른쪽으로 바라보게 설정
            }
        }
    }

    void CheckGround()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(capCol2D.bounds.center, capCol2D.bounds.size, 0f, Vector2.down, 0.02f, LayerMask.GetMask("Ground"));
        isGround = raycastHit.collider != null; // 바닥이 있는지 여부 저장
    }

    bool CanMoveForward()
    {
        float dir = transform.localScale.x > 0 ? 1 : -1; // 바라보는 방향에 따라 레이 방향 설정
        Vector2 rayDirection = new Vector2(dir, 0);

        Vector2 rayStart = (Vector2)transform.position + new Vector2(dir * 0.5f, 0.2f); // 약간 위에서 발사

        // 레이캐스트를 발사하여 벽이 있는지 확인
        RaycastHit2D hit = Physics2D.Raycast(rayStart, rayDirection, 3.0f, LayerMask.GetMask("Ground")); // Wall 레이어로 레이캐스트
        if (hit)
        {
            Debug.Log("어 맞았다!!!");
        }
        Debug.DrawRay(rayStart, rayDirection * 0.5f, Color.red); // 디버그용 레이 표시

        return hit.collider == null; // 벽이 없으면 이동 가능
    }
    void LoadEffectPrefab()
    {
        if (enemy.enemyName == "Eagle")
        {
            // PunchffectPrefab을 Resources 폴더에서 로드
            AttackEffectPrefab = Resources.Load<GameObject>("Enemy/Eagle/Eagle_Tornado");
            if (AttackEffectPrefab == null) Debug.LogError("AttackEffect 프리팹을 찾을 수 없습니다! 경로를 확인하세요.");
            else if (AttackEffectPrefab) Debug.Log("AttackEffect 프리팹 로드 완료.");
        }

        // else if (enemy.enemyName == "Bomber")
        // {
        //     // Bomber Attack Effect를 Resources 폴더에서 로드
        //     AttackEffectPrefab = Resources.Load<GameObject>("Enemy/Bomber_Bomb");
        //     if (AttackEffectPrefab == null) Debug.LogError("AttackEffect 프리팹을 찾을 수 없습니다! 경로를 확인하세요.");
        //     else if (AttackEffectPrefab) Debug.Log("AttackEffect 프리팹 로드 완료.");
        // }
    }

    private IEnumerator BomberEffectBounce(GameObject effect)
    {
        
        float bounceHeight = 0.5f; // 튀는 최고 높이
        float bounceDuration = 0.3f; // 한 번 튀는 데 걸리는 시간
        int bounceCount = 3; // 튀는 횟수
        float moveDistance = 0.5f; // 수평으로 이동할 거리

        // 이동 방향 계산 (플레이어가 오른쪽을 보고 있다면 양수, 왼쪽이라면 음수)
        float moveDirection = transform.localScale.x > 0 ? -1 : 1;

        Vector3 initialPosition = effect.transform.position + new Vector3(-0.5f, -0.5f, 0);

        for (int i = 0; i < bounceCount; i++)
        {
            // 목표 위치 (포물선 이동의 끝점)
            Vector3 targetPosition = initialPosition + new Vector3(moveDirection * moveDistance, 0, 0); // Y축 조금 낮게 설정
            yield return ParabolicMoveEffect(effect, initialPosition, targetPosition, bounceHeight, bounceDuration);

            // 이동 기준점 갱신
            initialPosition = targetPosition;
        }

        // 마지막 튀기 후 제거
        Destroy(effect);
    }

    private IEnumerator ParabolicMoveEffect(GameObject effect, Vector3 startPosition, Vector3 targetPosition, float height, float duration)
    {
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // 수평 위치: 선형 보간
            float x = Mathf.Lerp(startPosition.x, targetPosition.x, t);

            // 수직 위치: 포물선 형태
            float y = Mathf.Lerp(startPosition.y, targetPosition.y, t) + height * Mathf.Sin(Mathf.PI * t);

            // 새 위치 적용
            effect.transform.position = new Vector3(x, y, startPosition.z);
            yield return null;
        }

        // 마지막 위치 정밀하게 설정
        effect.transform.position = targetPosition;
    }
}