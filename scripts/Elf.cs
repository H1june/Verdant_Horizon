using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Elf : MonoBehaviour
{
    Animator animator;
    Rigidbody2D rigid2D;
    BoxCollider2D col2D;

    public GameObject SwiftbladeEffectPrefab; // 펀치 이펙트 프리팹
    public GameObject WaterEffectPrefab; // 물 이펙트 프리팹
    public GameObject SwordAuraEffectPrefab; // 검기 준비 이펙트 프리팹
    public GameObject projectileSwordAuraEffectPrefab; // 검기 준비 이펙트 프리팹
    public GameObject ChargeEffectPrefab; // 돌진 이펙트 프리팹
    public GameObject ShieldReadyEffectPrefab; // 배리어 준비 이펙트 프리팹
    public GameObject ShieldEffectPrefab; // 배리어 이펙트 프리팹
    public GameObject FlameEffectPrefab; // 불 이펙트 프리팹
    public GameObject NatureEffectPrefab; // 불 이펙트 프리팹
    public GameObject WindEffectPrefab; // 바람 이펙트 프리팹

    // 스탯
    public int playerLevel = 1;
    public int maxHp = 100;
    public int nowHp = 100;
    public int atkDmg = 15;
    public int aviltyPower = 15;
    public int maxExp = 50;
    public int nowExp = 0;
    public float atkSpeed = 1.0f;
    public float moveSpeed = 5.0f;
    public float jumpPower = 15.0f;
    public Image nowHpbar;

    // 플레이어 컨트롤
    public bool isHit = false;
    public bool isSit = false;
    public bool attacked = false;
    public bool isJumping = false;
    private bool isJumpingDown = false; // 하강 중 여부
    private float previousYPosition; // 플레이어의 이전 y좌표 저장
    public bool isDead = false;

    // 점프 관련
    public bool inputJump = false;
    public bool isGrounded = false;
    public int jumpCount = 0;
    public int maxJumpCount = 2;

    // 리스폰 관련
    public Vector3 respawnPosition;
    public float fallThreshold = -10f;

    // 이동 관련 입력
    public bool canControl = true;
    public bool inputRight = false;
    public bool inputLeft = false;
    private Vector3 lockedPosition;
    private bool isPositionLocked = false;

    // Dash
    public bool isDash = false;
    public bool isInvincible = false; // 무적 상태
    public float dashDistance = 500.0f; // 대시 거리
    public float dashCooldown = 0.0f; // 대시 쿨타임
    private bool canDash = true;

    // 이동 거리 계산 관련 변수
    private Vector3 startPosition;  // 플레이어의 시작 위치

    private Vector3 previousPosition;
    public float traveledDistance;

    public float distanceToLevelUp = 100000000.0f;  // 레벨업에 필요한 이동 거리

    void AttackTrue()
    {
        attacked = true;
    }

    void AttackFalse()
    {
        attacked = false;
    }

    public void Start()
    {
        respawnPosition = new Vector3(-23.3f, -1.5f, 0);  // 초기 위치 설정
        transform.position = respawnPosition;

        // 컴포넌트 초기화
        animator = GetComponent<Animator>();
        rigid2D = GetComponent<Rigidbody2D>();
        col2D = GetComponent<BoxCollider2D>();

        attacked = false;
        previousYPosition = transform.position.y; // 포지션 초기화

        // 초기 위치 저장
        previousPosition = transform.position;
        traveledDistance = 0f;

        // 초기 스탯 설정
        SetAttackSpeed(1.5f);

        // 이펙트 프리팹 가져오기
        LoadEffectPrefab();
    }

    public void Update()
    {
        // 체력 바 업데이트
        nowHpbar.fillAmount = (float)nowHp / (float)maxHp;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("최대 HP : " + maxHp);
            Debug.Log("현재 HP : " + nowHp);
            Debug.Log("냉철 : " + atkDmg); //AD
            Debug.Log("친화 : " + aviltyPower); // AP
            Debug.Log("공격 속도 : " + atkSpeed);
            Debug.Log("이동 속도 : " + moveSpeed);
            Debug.Log("최대 경험치 : " + maxExp);
            Debug.Log("현재 경험치 : " + nowExp);
        }

        if (canControl && !isSit)
        {
            // 이동 입력 처리
            if (Input.GetKey(KeyCode.RightArrow))
            {
                inputRight = true;
                inputLeft = false;  // 한 방향만 이동 가능
                transform.localScale = new Vector3(1, 1, 1);  // 캐릭터가 오른쪽을 바라봄
                animator.SetBool("moving", true);
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                inputLeft = true;
                inputRight = false;  // 한 방향만 이동 가능
                transform.localScale = new Vector3(-1, 1, 1);  // 캐릭터가 왼쪽을 바라봄
                animator.SetBool("moving", true);
            }
            else
            {
                inputRight = false;
                inputLeft = false;
                animator.SetBool("moving", false);
            }

            // 점프 (Space Bar)
            if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
            {
                if (isSit)
                {
                    // 앉기 상태에서 점프 시작
                    isJumpingDown = true; // 하강 상태로 설정
                    animator.SetTrigger("Downjump");
                    isTriggerOn(); // 바로 isTrigger를 켜고

                    Invoke("isTriggerOff", 0.2f); // 0.5초 후에 isTrigger를 끄기
                }

                else if (jumpCount < maxJumpCount)
                {
                    inputJump = true;
                    jumpCount++;  // 점프 횟수 증가
                    animator.SetBool("jump_up", true);
                }
            }

            // 바닥 체크 (Raycast 사용)
            RaycastHit2D raycastHit = Physics2D.BoxCast(col2D.bounds.center, col2D.bounds.size, 0f, Vector2.down, 0.02f, LayerMask.GetMask("Ground"));
            isGrounded = raycastHit.collider != null;

            // === 점프 애니메이션 컨트롤 ===
            float verticalVelocity = rigid2D.velocity.y;
            if (!isGrounded)
            {
                if (verticalVelocity > 0.1)
                {
                    animator.SetBool("jump_up", true);
                    animator.SetBool("jump_under", false);
                }
                else if (verticalVelocity <= 0.1)
                {
                    animator.SetBool("jump_up", false);
                    animator.SetBool("jump_under", true);
                }
            }

            else
            {
                animator.SetBool("jump_up", false);
                animator.SetBool("jump_under", false);
            }

            // 땅에 있을 때 점프 카운트 리셋
            if (isGrounded && jumpCount > 0)
            {
                jumpCount = 0;
            }

            // 대쉬 처리 (D키)
            if (Input.GetKeyDown(KeyCode.D) && !isDash)
            {
                Dash();
            }

            // 공격 처리 (Q키)
            if (Input.GetKeyDown(KeyCode.Q))
            {
                atkDmg = 5; // 평타 데미지
                rigid2D.velocity = new Vector2(rigid2D.velocity.x, 0); // y축 속도 초기화
            }

            // 연속베기 스킬(C키)
            if (Input.GetKeyDown(KeyCode.C) && !animator.GetCurrentAnimatorStateInfo(0).IsName("elf_swiftBlade"))
            {
                atkDmg = 5;
                animator.SetTrigger("swiftBlade");
                canControl = false;
                LockPosition();
                Invoke("EnableControl", 1.917f);
                Invoke("UnlockPosition", 1.917f);
                Invoke("SpawnSwiftBladeEffect", 0.15f);
            }

            // 물 스킬(S키)
            if (Input.GetKeyDown(KeyCode.S) && !animator.GetCurrentAnimatorStateInfo(0).IsName("elf_water"))
            {
                atkDmg = 7;
                animator.SetTrigger("skill_water");
                canControl = false;
                LockPosition();
                Invoke("UnlockPosition", 1.0f);
                Invoke("EnableControl", 1.0f);
                SpawnWaterEffect();
            }

            // 검기 스킬(X키)
            if (Input.GetKeyDown(KeyCode.X) && !animator.GetCurrentAnimatorStateInfo(0).IsName("elf_sword_aura"))
            {
                atkDmg = 15;
                animator.SetTrigger("sword_aura");
                canControl = false;
                LockPosition();
                Invoke("UnlockPosition", 1.083f);
                Invoke("EnableControl", 1.083f);
                SpawnSwordAuraEffect();
                Invoke("projectileSpawnSwordAuraEffect", 0.7f);
            }

            // 돌진 스킬(V키)
            if (Input.GetKeyDown(KeyCode.V) && !animator.GetCurrentAnimatorStateInfo(0).IsName("elf_charge"))
            {
                atkDmg = 9;
                animator.SetTrigger("charge");
                canControl = false;
                Invoke("EnableControl", 0.583f);
                Invoke("SpawnChargeEffect", 0.3f);
                Invoke("chargeMoveDealy", 0.3f); ;
            }

            // 실드 스킬(Z키)
            if (Input.GetKeyDown(KeyCode.Z) && !animator.GetCurrentAnimatorStateInfo(0).IsName("elf_shield"))
            {
                atkDmg = 6;
                animator.SetTrigger("shield");
                canControl = false;
                LockPosition();
                Invoke("UnlockPosition", 1.166f);
                SpawnShieldReadyEffect();
                Invoke("EnableControl", 1.166f);
                Invoke("SpawnShieldEffect", 1.3f);
            }

            // 화염 스킬 (A키)
            if (Input.GetKeyDown(KeyCode.A) && !animator.GetCurrentAnimatorStateInfo(0).IsName("elf_flame"))
            {
                atkDmg = 10;
                animator.SetTrigger("flame");
                canControl = false;
                LockPosition();
                Invoke("UnlockPosition", 1.45f);
                Invoke("EnableControl", 1.45f);
                SpawnFlameEffect();
            }

            // 자연 스킬 (F키)
            if (Input.GetKeyDown(KeyCode.F) && !animator.GetCurrentAnimatorStateInfo(0).IsName("elf_nature"))
            {
                atkDmg = 0;
                animator.SetTrigger("nature");
                canControl = false;
                LockPosition();
                Invoke("UnlockPosition", 0.25f);
                Invoke("EnableControl", 0.25f);
                SpawnNatureEffect();
                natureHeal();
            }

            // 바람 스킬 (W키)
            if (Input.GetKeyDown(KeyCode.W) && !animator.GetCurrentAnimatorStateInfo(0).IsName("elf_wind"))
            {
                atkDmg = 10;
                animator.SetTrigger("wind");
                canControl = false;
                LockPosition();
                Invoke("UnlockPosition", 1.16f);
                Invoke("EnableControl", 1.16f);
                SpawnWindEffect();
            }

            // 플레이어가 일정 y 값 아래로 떨어지면 리스폰
            if (transform.position.y < fallThreshold)
            {
                Respawn();
            }

            // 현재 위치와 이전 위치의 거리 계산
            float distanceThisFrame = Vector3.Distance(previousPosition, transform.position);
            traveledDistance += distanceThisFrame;

            // 현재 위치를 이전 위치로 업데이트
            previousPosition = transform.position;
        }

        // 플레이어 사망
        if (nowHp <= 0)
        {
            Die();
        }

        // 앉기 (↓)
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            isSit = true;
            animator.SetBool("sitting", true);
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            isSit = false;
            animator.SetBool("sitting", false);
        }

        // 하단점프
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            if (isSit)
            {
                // 앉기 상태에서 점프 시작
                Debug.Log("하단점프 실시");
                isJumpingDown = true; // 하강 상태로 설정
                animator.SetTrigger("Downjump");
                isTriggerOn(); // 바로 isTrigger를 켜고

                // 0.5초 후에 isTrigger를 끄는 함수 호출
                Invoke("isTriggerOff", 0.2f);
            }
        }

        // 일정 거리를 이동했을 때 레벨업 패널을 띄우기
        // CheckDistanceForLevelUp();
    }
    
    private void FixedUpdate()
    {
        if (isPositionLocked)
        {
            // 고정된 위치로 이동 강제
            transform.position = lockedPosition;
        }
        else if (!isDash)
        {
            if (inputRight)
            {
                rigid2D.velocity = new Vector2(moveSpeed, rigid2D.velocity.y);
            }
            else if (inputLeft)
            {
                rigid2D.velocity = new Vector2(-moveSpeed, rigid2D.velocity.y);
            }
            else
            {
                rigid2D.velocity = new Vector2(0, rigid2D.velocity.y);
            }
        }

        // 점프 처리
        if (inputJump)
        {
            inputJump = false;
            rigid2D.velocity = new Vector2(rigid2D.velocity.x, jumpPower);
        }
    }

    void EnableControl()
    {
        canControl = true;
    }

    void LockPosition()
    {
        lockedPosition = transform.position;
        isPositionLocked = true;
        rigid2D.isKinematic = true;  // 물리적 계산을 멈춤

        // 중력을 0으로 설정 (스킬 중에만)
        rigid2D.gravityScale = 5;
    }

    void UnlockPosition()
    {
        isPositionLocked = false;
        rigid2D.isKinematic = false;  // 물리적 계산을 재개

        // 중력을 원래대로 복원
        rigid2D.gravityScale = 5;  // 기본 중력 값으로 복원 (적절히 조정)
    }

    private float fixedYPosition;  // 대시 중 고정된 Y좌표 저장

    void Dash()
    {
        if (canDash)
        {
            isDash = true;
            isInvincible = true;  // 대시 중 무적 상태
            canDash = false;
            animator.SetBool("dashing", true);

            // 대시 시작 시 Y좌표 저장
            fixedYPosition = transform.position.y;

            float dashDirection = transform.localScale.x;  // 캐릭터가 보고 있는 방향으로 대시
            rigid2D.velocity = new Vector2(dashDirection * dashDistance, 0);  // Y축 속도 0으로 고정

            // 대시 동안 Y좌표 고정
            InvokeRepeating("LockYPosition", 0f, Time.fixedDeltaTime);

            Invoke("EndDash", 0.2f);  // 0.2초 후 대시 종료
            Invoke("ResetDash", dashCooldown);  // 대시 쿨타임 적용
        }
    }

    void LockYPosition()
    {
        // 고정된 Y좌표 유지
        transform.position = new Vector3(transform.position.x, fixedYPosition, transform.position.z);
    }

    void EndDash()
    {
        // 대시 종료 시 모든 동작 정리
        CancelInvoke("LockYPosition");  // Y좌표 고정 중지
        rigid2D.velocity = Vector2.zero;  // 대시 후 속도 초기화
        isDash = false;
        isInvincible = false;
        animator.SetBool("dashing", false);
    }



    void ResetDash()
    {
        canDash = true;  // 대시 가능 상태로 복구
    }

    void Respawn()
    {
        // 리스폰 시 초기 위치로 이동
        transform.position = respawnPosition;
        rigid2D.velocity = Vector2.zero;

        // 체력 감소 처리
        nowHp -= 10;
        if (nowHp < 0)
        {
            nowHp = 0;

        }

        // 체력 바 업데이트
        nowHpbar.fillAmount = (float)nowHp / (float)maxHp;
    }

    void SpawnEffect(GameObject effectPrefab)
    {
        if (effectPrefab != null)
        {
            // 이펙트 생성 위치 계산 (캐릭터의 위치에서 앞으로)
            Vector3 spawnPosition = transform.position;

            // 이펙트 생성
            GameObject effect = Instantiate(effectPrefab, spawnPosition, Quaternion.identity);

            // 이펙트의 방향 설정 (플레이어가 왼쪽을 보면 이펙트를 뒤집음)
            Vector3 effectScale = effect.transform.localScale;
            effectScale.x = Mathf.Abs(effectScale.x) * (transform.localScale.x > 0 ? 1 : -1);

            effect.transform.localScale = effectScale;
        }
    }

    void SpawnSwiftBladeEffect()
    {
        if (SwiftbladeEffectPrefab != null)
        {
            // 이펙트 생성 위치 계산 (캐릭터의 위치에서 앞으로)
            Vector3 spawnPosition = transform.position + new Vector3(transform.localScale.x * 1.5f, 0, 0);

            // 이펙트 생성
            GameObject effect = Instantiate(SwiftbladeEffectPrefab, spawnPosition, Quaternion.identity);

            // 이펙트의 방향 설정 (플레이어가 왼쪽을 보면 이펙트를 뒤집음)
            Vector3 effectScale = effect.transform.localScale;

            effectScale.x = Mathf.Abs(effectScale.x) * (transform.localScale.x > 0 ? 1 : -1);

            effect.transform.localScale = effectScale;

            effect.transform.SetParent(transform);
        }
    }

    void SpawnWaterEffect()
    {
        if (WaterEffectPrefab != null)
        {
            // 이펙트 생성 위치 계산 (캐릭터의 위치에서 앞으로)
            Vector3 spawnPosition1 = transform.position + new Vector3(transform.localScale.x * 1.0f, 0, 0);
            Vector3 spawnPosition2 = transform.position + new Vector3(transform.localScale.x * 1.0f, 0, 0);
            Vector3 spawnPosition3 = transform.position + new Vector3(transform.localScale.x * 1.0f, 0, 0);

            // 이펙트 생성
            GameObject effect1 = Instantiate(WaterEffectPrefab, spawnPosition1, Quaternion.identity);
            GameObject effect2 = Instantiate(WaterEffectPrefab, spawnPosition1, Quaternion.identity);
            GameObject effect3 = Instantiate(WaterEffectPrefab, spawnPosition1, Quaternion.identity);

            // 이펙트의 방향 설정 (플레이어가 왼쪽을 보면 이펙트를 뒤집음)
            Vector3 effect1Scale = effect1.transform.localScale;
            Vector3 effect2Scale = effect2.transform.localScale;
            Vector3 effect3Scale = effect3.transform.localScale;

            effect1Scale.x = Mathf.Abs(effect1Scale.x) * (transform.localScale.x > 0 ? 1 : -1);
            effect2Scale.x = Mathf.Abs(effect2Scale.x) * (transform.localScale.x > 0 ? 1 : -1);
            effect3Scale.x = Mathf.Abs(effect3Scale.x) * (transform.localScale.x > 0 ? 1 : -1);
    
            effect1.transform.localScale = effect1Scale;
            effect2.transform.localScale = effect2Scale;
            effect3.transform.localScale = effect3Scale;

            effect1.transform.SetParent(transform);
            effect2.transform.SetParent(transform);
            effect3.transform.SetParent(transform);

            // HomingMissile 컴포넌트가 있는 경우 초기화
            HomingMissile homingMissile1 = effect1.GetComponent<HomingMissile>();
            HomingMissile homingMissile2 = effect2.GetComponent<HomingMissile>();
            HomingMissile homingMissile3 = effect3.GetComponent<HomingMissile>();
        }
    }

    void SpawnSwordAuraEffect()
    {
        if (SwordAuraEffectPrefab != null)
        {
            // 이펙트 생성 위치 계산 (캐릭터의 위치에서 앞으로)
            Vector3 spawnPosition = transform.position + new Vector3(transform.localScale.x * 1.0f, 0, 0);

            // 이펙트 생성
            GameObject effect = Instantiate(SwordAuraEffectPrefab, spawnPosition, Quaternion.identity);

            // 이펙트의 방향 설정 (플레이어가 왼쪽을 보면 이펙트를 뒤집음)
            Vector3 effectScale = effect.transform.localScale;
            effectScale.x = Mathf.Abs(effectScale.x) * (transform.localScale.x > 0 ? 1 : -1);

            effect.transform.localScale = effectScale;

            effect.transform.SetParent(transform);
        }
    }

    void projectileSpawnSwordAuraEffect()
    {
        if (projectileSwordAuraEffectPrefab != null)
        {
            // 이펙트 생성 위치 계산 (캐릭터의 위치에서 앞으로)
            Vector3 spawnPosition = transform.position + new Vector3(transform.localScale.x * 1.0f, 0, 0);

            // 이펙트 생성
            GameObject effect = Instantiate(projectileSwordAuraEffectPrefab, spawnPosition, Quaternion.identity);

            // 이펙트의 방향 설정 (플레이어가 왼쪽을 보면 이펙트를 뒤집음)
            Vector3 effectScale = effect.transform.localScale;
            effectScale.x = Mathf.Abs(effectScale.x) * (transform.localScale.x > 0 ? 1 : -1);

            effect.transform.localScale = effectScale;
        }
    }

    void SpawnChargeEffect()
    {
        if (ChargeEffectPrefab != null)
        {
            // 이펙트 생성 위치 계산 (캐릭터의 위치에서 앞으로)
            Vector3 spawnPosition = transform.position;

            // 이펙트 생성
            GameObject effect = Instantiate(ChargeEffectPrefab, spawnPosition, Quaternion.identity);

            // 이펙트의 방향 설정 (플레이어가 왼쪽을 보면 이펙트를 뒤집음)
            Vector3 effectScale = effect.transform.localScale;
            effectScale.x = Mathf.Abs(effectScale.x) * (transform.localScale.x > 0 ? 1 : -1);

            effect.transform.localScale = effectScale;
        }
    }

    void SpawnShieldReadyEffect()
    {
        if (ShieldReadyEffectPrefab != null)
        {
            // 이펙트 생성 위치 계산 (캐릭터의 위치에서 앞으로)
            Vector3 spawnPosition = transform.position;

            // 이펙트 생성
            GameObject effect = Instantiate(ShieldReadyEffectPrefab, spawnPosition, Quaternion.identity);

            // 이펙트의 방향 설정 (플레이어가 왼쪽을 보면 이펙트를 뒤집음)
            Vector3 effectScale = effect.transform.localScale;
            effectScale.x = Mathf.Abs(effectScale.x) * (transform.localScale.x > 0 ? 1 : -1);

            effect.transform.localScale = effectScale;

            effect.transform.SetParent(transform);
        }
    }

    void SpawnShieldEffect()
    {
        if (ShieldEffectPrefab != null)
        {
            // 이펙트 생성 위치 계산 (캐릭터의 위치에서)
            Vector3 spawnPosition = transform.position;

            // 이펙트 생성
            GameObject effect = Instantiate(ShieldEffectPrefab, spawnPosition, Quaternion.identity);

            // 이펙트의 방향 설정 (플레이어가 왼쪽을 보면 이펙트를 뒤집음)
            Vector3 effectScale = effect.transform.localScale;
            effectScale.x = Mathf.Abs(effectScale.x) * (transform.localScale.x > 0 ? 1 : -1);

            effect.transform.localScale = effectScale;

            // 플레이어를 부모로 설정하여 이펙트를 따라다니도록 만듦
            effect.transform.SetParent(transform);
        }
    }

    void SpawnFlameEffect()
    {
        if (FlameEffectPrefab != null)
        {
            // 이펙트 생성 위치 계산 (캐릭터의 위치에서)
            Vector3 spawnPosition = transform.position;

            // 이펙트 생성
            GameObject effect = Instantiate(FlameEffectPrefab, spawnPosition, Quaternion.identity);

            // 이펙트의 방향 설정 (플레이어가 왼쪽을 보면 이펙트를 뒤집음)
            Vector3 effectScale = effect.transform.localScale;
            effectScale.x = Mathf.Abs(effectScale.x) * (transform.localScale.x > 0 ? 1 : -1);

            effect.transform.localScale = effectScale;

            // 플레이어를 부모로 설정하여 이펙트를 따라다니도록 만듦
            effect.transform.SetParent(transform);
        }
    }

    void SpawnNatureEffect()
    {
        if (NatureEffectPrefab != null)
        {
            // 이펙트 생성 위치 계산 (캐릭터의 위치에서)
            Vector3 spawnPosition = transform.position + new Vector3(transform.localScale.x * -0.1f, transform.localScale.y * 1.3f, 0);

            // 이펙트 생성
            GameObject effect = Instantiate(NatureEffectPrefab, spawnPosition, Quaternion.identity);

            // 이펙트의 방향 설정 (플레이어가 왼쪽을 보면 이펙트를 뒤집음)
            Vector3 effectScale = effect.transform.localScale;
            effectScale.x = Mathf.Abs(effectScale.x) * (transform.localScale.x > 0 ? 1 : -1);

            effect.transform.localScale = effectScale;

            // 플레이어를 부모로 설정하여 이펙트를 따라다니도록 만듦
            effect.transform.SetParent(transform);
        }
    }

    void SpawnWindEffect()
    {
        if (WindEffectPrefab != null)
        {
            // 이펙트 생성 위치 계산 (캐릭터의 위치에서)
            Vector3 spawnPosition = transform.position;

            // 이펙트 생성
            GameObject effect = Instantiate(WindEffectPrefab, spawnPosition, Quaternion.identity);

            // 이펙트의 방향 설정 (플레이어가 왼쪽을 보면 이펙트를 뒤집음)
            Vector3 effectScale = effect.transform.localScale;
            effectScale.x = Mathf.Abs(effectScale.x) * (transform.localScale.x > 0 ? 1 : -1);

            effect.transform.localScale = effectScale;

            // 플레이어를 부모로 설정하여 이펙트를 따라다니도록 만듦
            effect.transform.SetParent(transform);
        }
    }

    public void TakeDamage(int damage, Transform attacker)
    {
        // 현재 애니메이션 상태를 가져옵니다.
        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);

        // 특정 애니메이션이 재생 중인지 확인합니다.
        if (currentState.IsName("elf_swiftBlade") || currentState.IsName("elf_water")    ||
            currentState.IsName("elf_sword_aura") || currentState.IsName("elf_charge")   ||
            currentState.IsName("elf_shield")     || currentState.IsName("elf_nature")   ||
            currentState.IsName("elf_flame")      || currentState.IsName("elf_air")     ||
            currentState.IsName("comboAttack1")   || currentState.IsName("comboAttack2") ||
            currentState.IsName("comboAttack3")   || currentState.IsName("comboAttack4") ||
            currentState.IsName("elf_dash")       || currentState.IsName("elf_sit"))
        {
            // 특정 애니메이션이 실행 중일 경우 데미지를 받을 수 없도록 합니다.
            return;
        }

        if (!isHit)  // 이미 피격 상태가 아니면
        {
            nowHp -= damage;  // 체력 감소
            if (nowHp < 0)
            {
                nowHp = 0;  // 체력 최소화
            }

            nowHpbar.fillAmount = (float)nowHp / (float)maxHp;  // 체력바 업데이트

            // hit 애니메이션 재생
            animator.SetTrigger("hit");
            isHit = true;

            // 넉백 처리
            // KnockBack(attacker);

            // 일정 시간 후 다시 피격 가능 상태로 변경
            Invoke("ResetHitState", 1.0f);  // 예시로 1초 동안 피격 불가
        }
    }

    public void natureHeal()
    {
        nowHp = nowHp += 20;
    }

    void KnockBack(Transform attacker)
    {
        float knockBackForce = 5.0f;  // 넉백 힘을 줄여서 현실적인 이동 구현
        float knockBackDuration = 0.05f; // 넉백 지속 시간

        // 넉백 방향 결정 (적이 왼쪽에 있으면 오른쪽으로, 오른쪽에 있으면 왼쪽으로)
        float knockBackDirection = transform.position.x - attacker.position.x > 0 ? 1 : -1;

        // 넉백 적용 (X축으로만 밀어내기)
        rigid2D.velocity = new Vector2(knockBackDirection * knockBackForce, rigid2D.velocity.y);

        // 일정 시간 후 속도 초기화 (넉백이 끝난 후)
        Invoke("ResetVelocity", knockBackDuration);
    }

    void ResetVelocity()
    {
        rigid2D.velocity = new Vector2(0, rigid2D.velocity.y);  // X축 속도를 0으로 초기화
    }

    void ResetHitState()
    {
        isHit = false;  // 피격 가능 상태로 복구
    }

    void SetAttackSpeed(float speed)
    {
        animator.SetFloat("attackSpeed", speed);
        atkSpeed = speed;
    }

    public void Die()
    {
        animator.SetTrigger("die");            // die 애니메이션 실행
        Destroy(gameObject, 3);                // 3초후 제거
        isDead = true;
        canControl = false;
    }

    public void isTriggerOn()
    {
        col2D.isTrigger = true;
    }

    public void isTriggerOff()
    {
        col2D.isTrigger = false;
    }

    void CheckDistanceForLevelUp()
    {
        // 일정 거리를 넘으면 레벨업 패널 표시
        if (traveledDistance >= distanceToLevelUp)
        {
            LevelUp();
            playerLevel++;
            startPosition = transform.position;  // 거리 초기화 (추가 레벨업 가능하게)
        }
    }

    void LevelUp()
    {
        // 레벨업 패널 표시 (LevelUpUI 스크립트의 ShowLevelUpPanel 함수 호출)
        LevelUpUI levelUpUI = FindObjectOfType<LevelUpUI>();
        if (levelUpUI != null)
        {
            levelUpUI.ShowLevelUpPanel(playerLevel);
            Debug.Log("레벨업! 이동 거리를 달성했습니다.");
        }
        else
        {
            Debug.LogError("LevelUpUI를 찾을 수 없습니다.");
        }
    }

    void LoadEffectPrefab()
    {
        // SwiftbladeEffectPrefab을 Resources 폴더에서 로드
        SwiftbladeEffectPrefab = Resources.Load<GameObject>("Elf/Skill_Effect/Elf_Mech_Swiftblade_Effect");
        if (SwiftbladeEffectPrefab == null) Debug.LogError("SwiftbladeEffect 프리팹을 찾을 수 없습니다! 경로를 확인하세요.");
        else if (SwiftbladeEffectPrefab) Debug.Log("SwiftbladeEffect 프리팹 로드 완료.");

        // SwiftBladeffectPrefab을 Resources 폴더에서 로드
        WaterEffectPrefab = Resources.Load<GameObject>("Elf/Skill_Effect/Projectile Test");
        if (WaterEffectPrefab == null) Debug.LogError("WaterEffect 프리팹을 찾을 수 없습니다! 경로를 확인하세요.");
        else if (WaterEffectPrefab) Debug.Log("WaterEffect 프리팹 로드 완료.");

        // SwordAuraEffectPrefab을 Resources 폴더에서 로드
        SwordAuraEffectPrefab = Resources.Load<GameObject>("Elf/Skill_Effect/Elf_Mech_SwordAura_Effect");
        if (SwordAuraEffectPrefab == null) Debug.LogError("SwordAuraEffect 프리팹을 찾을 수 없습니다! 경로를 확인하세요.");
        else if (SwordAuraEffectPrefab) Debug.Log("SwordAuraEffect 프리팹 로드 완료.");

        // projectileSwordAuraEffectPrefab을 Resources 폴더에서 로드
        projectileSwordAuraEffectPrefab = Resources.Load<GameObject>("Elf/Skill_Effect/Projectile_Mech_SwordAura");
        if (projectileSwordAuraEffectPrefab == null) Debug.LogError("projectileSwordAuraEffect 프리팹을 찾을 수 없습니다! 경로를 확인하세요.");
        else if (projectileSwordAuraEffectPrefab) Debug.Log("projectileSwordAuraEffect 프리팹 로드 완료.");

        // ChargeEffectPrefab을 Resources 폴더에서 로드
        ChargeEffectPrefab = Resources.Load<GameObject>("Elf/Skill_Effect/Elf_Mech_Charge_Effect");
        if (ChargeEffectPrefab == null) Debug.LogError("ChargeEffect 프리팹을 찾을 수 없습니다! 경로를 확인하세요.");
        else if (ChargeEffectPrefab) Debug.Log("ChargeEffect 프리팹 로드 완료.");

        // ShieldReadyEffectPrefab을 Resources 폴더에서 로드
        ShieldReadyEffectPrefab = Resources.Load<GameObject>("Elf/Skill_Effect/Elf_Mech_Shield_Ready_Effect");
        if (ShieldReadyEffectPrefab == null) Debug.LogError("ShieldReadyEffect 프리팹을 찾을 수 없습니다! 경로를 확인하세요.");
        else if (ShieldReadyEffectPrefab) Debug.Log("ShieldReadyEffect 프리팹 로드 완료.");

        // ShieldEffectPrefab을 Resources 폴더에서 로드
        ShieldEffectPrefab = Resources.Load<GameObject>("Elf/Skill_Effect/Elf_Mech_Shield_Effect");
        if (ShieldEffectPrefab == null) Debug.LogError("ShieldEffect 프리팹을 찾을 수 없습니다! 경로를 확인하세요.");
        else if (ShieldEffectPrefab) Debug.Log("ShieldEffect 프리팹 로드 완료.");

        // FlameEffectPrefab을 Resources 폴더에서 로드
        FlameEffectPrefab = Resources.Load<GameObject>("Elf/Skill_Effect/Elf_Magic_Flame_Effect");
        if (FlameEffectPrefab == null) Debug.LogError("FlameEffect 프리팹을 찾을 수 없습니다! 경로를 확인하세요.");
        else if (FlameEffectPrefab) Debug.Log("FlameEffect 프리팹 로드 완료.");

        // NatureEffectPrefab을 Resources 폴더에서 로드
        NatureEffectPrefab = Resources.Load<GameObject>("Elf/Skill_Effect/Elf_Tree_Nature_Effect");
        if (NatureEffectPrefab == null) Debug.LogError("NatureEffect 프리팹을 찾을 수 없습니다! 경로를 확인하세요.");
        else if (NatureEffectPrefab) Debug.Log("NatureEffect 프리팹 로드 완료.");

        // WindEffectPrefab을 Resources 폴더에서 로드
        WindEffectPrefab = Resources.Load<GameObject>("Elf/Skill_Effect/Elf_Magic_Wind");
        if (WindEffectPrefab == null) Debug.LogError("WindEffect 프리팹을 찾을 수 없습니다! 경로를 확인하세요.");
        else if (WindEffectPrefab) Debug.Log("WindEffect 프리팹 로드 완료.");
    }

    void chargeMoveDealy() 
    {
        // 일정 거리만큼 이동 (거리: 5f)
        float moveDistance = 3f;
        Vector3 moveDirection = transform.right * (transform.localScale.x > 0 ? 1 : -1); // 현재 바라보는 방향(양의 x 축이면 오른쪽, 음의 x 축이면 왼쪽)
        transform.position += moveDirection * moveDistance;
    }
}
