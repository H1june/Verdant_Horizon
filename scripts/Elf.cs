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

    public GameObject SwiftbladeEffectPrefab; // ��ġ ����Ʈ ������
    public GameObject WaterEffectPrefab; // �� ����Ʈ ������
    public GameObject SwordAuraEffectPrefab; // �˱� �غ� ����Ʈ ������
    public GameObject projectileSwordAuraEffectPrefab; // �˱� �غ� ����Ʈ ������
    public GameObject ChargeEffectPrefab; // ���� ����Ʈ ������
    public GameObject ShieldReadyEffectPrefab; // �踮�� �غ� ����Ʈ ������
    public GameObject ShieldEffectPrefab; // �踮�� ����Ʈ ������
    public GameObject FlameEffectPrefab; // �� ����Ʈ ������
    public GameObject NatureEffectPrefab; // �� ����Ʈ ������
    public GameObject WindEffectPrefab; // �ٶ� ����Ʈ ������

    // ����
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

    // �÷��̾� ��Ʈ��
    public bool isHit = false;
    public bool isSit = false;
    public bool attacked = false;
    public bool isJumping = false;
    private bool isJumpingDown = false; // �ϰ� �� ����
    private float previousYPosition; // �÷��̾��� ���� y��ǥ ����
    public bool isDead = false;

    // ���� ����
    public bool inputJump = false;
    public bool isGrounded = false;
    public int jumpCount = 0;
    public int maxJumpCount = 2;

    // ������ ����
    public Vector3 respawnPosition;
    public float fallThreshold = -10f;

    // �̵� ���� �Է�
    public bool canControl = true;
    public bool inputRight = false;
    public bool inputLeft = false;
    private Vector3 lockedPosition;
    private bool isPositionLocked = false;

    // Dash
    public bool isDash = false;
    public bool isInvincible = false; // ���� ����
    public float dashDistance = 500.0f; // ��� �Ÿ�
    public float dashCooldown = 0.0f; // ��� ��Ÿ��
    private bool canDash = true;

    // �̵� �Ÿ� ��� ���� ����
    private Vector3 startPosition;  // �÷��̾��� ���� ��ġ

    private Vector3 previousPosition;
    public float traveledDistance;

    public float distanceToLevelUp = 100000000.0f;  // �������� �ʿ��� �̵� �Ÿ�

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
        respawnPosition = new Vector3(-23.3f, -1.5f, 0);  // �ʱ� ��ġ ����
        transform.position = respawnPosition;

        // ������Ʈ �ʱ�ȭ
        animator = GetComponent<Animator>();
        rigid2D = GetComponent<Rigidbody2D>();
        col2D = GetComponent<BoxCollider2D>();

        attacked = false;
        previousYPosition = transform.position.y; // ������ �ʱ�ȭ

        // �ʱ� ��ġ ����
        previousPosition = transform.position;
        traveledDistance = 0f;

        // �ʱ� ���� ����
        SetAttackSpeed(1.5f);

        // ����Ʈ ������ ��������
        LoadEffectPrefab();
    }

    public void Update()
    {
        // ü�� �� ������Ʈ
        nowHpbar.fillAmount = (float)nowHp / (float)maxHp;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("�ִ� HP : " + maxHp);
            Debug.Log("���� HP : " + nowHp);
            Debug.Log("��ö : " + atkDmg); //AD
            Debug.Log("ģȭ : " + aviltyPower); // AP
            Debug.Log("���� �ӵ� : " + atkSpeed);
            Debug.Log("�̵� �ӵ� : " + moveSpeed);
            Debug.Log("�ִ� ����ġ : " + maxExp);
            Debug.Log("���� ����ġ : " + nowExp);
        }

        if (canControl && !isSit)
        {
            // �̵� �Է� ó��
            if (Input.GetKey(KeyCode.RightArrow))
            {
                inputRight = true;
                inputLeft = false;  // �� ���⸸ �̵� ����
                transform.localScale = new Vector3(1, 1, 1);  // ĳ���Ͱ� �������� �ٶ�
                animator.SetBool("moving", true);
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                inputLeft = true;
                inputRight = false;  // �� ���⸸ �̵� ����
                transform.localScale = new Vector3(-1, 1, 1);  // ĳ���Ͱ� ������ �ٶ�
                animator.SetBool("moving", true);
            }
            else
            {
                inputRight = false;
                inputLeft = false;
                animator.SetBool("moving", false);
            }

            // ���� (Space Bar)
            if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
            {
                if (isSit)
                {
                    // �ɱ� ���¿��� ���� ����
                    isJumpingDown = true; // �ϰ� ���·� ����
                    animator.SetTrigger("Downjump");
                    isTriggerOn(); // �ٷ� isTrigger�� �Ѱ�

                    Invoke("isTriggerOff", 0.2f); // 0.5�� �Ŀ� isTrigger�� ����
                }

                else if (jumpCount < maxJumpCount)
                {
                    inputJump = true;
                    jumpCount++;  // ���� Ƚ�� ����
                    animator.SetBool("jump_up", true);
                }
            }

            // �ٴ� üũ (Raycast ���)
            RaycastHit2D raycastHit = Physics2D.BoxCast(col2D.bounds.center, col2D.bounds.size, 0f, Vector2.down, 0.02f, LayerMask.GetMask("Ground"));
            isGrounded = raycastHit.collider != null;

            // === ���� �ִϸ��̼� ��Ʈ�� ===
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

            // ���� ���� �� ���� ī��Ʈ ����
            if (isGrounded && jumpCount > 0)
            {
                jumpCount = 0;
            }

            // �뽬 ó�� (DŰ)
            if (Input.GetKeyDown(KeyCode.D) && !isDash)
            {
                Dash();
            }

            // ���� ó�� (QŰ)
            if (Input.GetKeyDown(KeyCode.Q))
            {
                atkDmg = 5; // ��Ÿ ������
                rigid2D.velocity = new Vector2(rigid2D.velocity.x, 0); // y�� �ӵ� �ʱ�ȭ
            }

            // ���Ӻ��� ��ų(CŰ)
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

            // �� ��ų(SŰ)
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

            // �˱� ��ų(XŰ)
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

            // ���� ��ų(VŰ)
            if (Input.GetKeyDown(KeyCode.V) && !animator.GetCurrentAnimatorStateInfo(0).IsName("elf_charge"))
            {
                atkDmg = 9;
                animator.SetTrigger("charge");
                canControl = false;
                Invoke("EnableControl", 0.583f);
                Invoke("SpawnChargeEffect", 0.3f);
                Invoke("chargeMoveDealy", 0.3f); ;
            }

            // �ǵ� ��ų(ZŰ)
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

            // ȭ�� ��ų (AŰ)
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

            // �ڿ� ��ų (FŰ)
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

            // �ٶ� ��ų (WŰ)
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

            // �÷��̾ ���� y �� �Ʒ��� �������� ������
            if (transform.position.y < fallThreshold)
            {
                Respawn();
            }

            // ���� ��ġ�� ���� ��ġ�� �Ÿ� ���
            float distanceThisFrame = Vector3.Distance(previousPosition, transform.position);
            traveledDistance += distanceThisFrame;

            // ���� ��ġ�� ���� ��ġ�� ������Ʈ
            previousPosition = transform.position;
        }

        // �÷��̾� ���
        if (nowHp <= 0)
        {
            Die();
        }

        // �ɱ� (��)
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

        // �ϴ�����
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            if (isSit)
            {
                // �ɱ� ���¿��� ���� ����
                Debug.Log("�ϴ����� �ǽ�");
                isJumpingDown = true; // �ϰ� ���·� ����
                animator.SetTrigger("Downjump");
                isTriggerOn(); // �ٷ� isTrigger�� �Ѱ�

                // 0.5�� �Ŀ� isTrigger�� ���� �Լ� ȣ��
                Invoke("isTriggerOff", 0.2f);
            }
        }

        // ���� �Ÿ��� �̵����� �� ������ �г��� ����
        // CheckDistanceForLevelUp();
    }
    
    private void FixedUpdate()
    {
        if (isPositionLocked)
        {
            // ������ ��ġ�� �̵� ����
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

        // ���� ó��
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
        rigid2D.isKinematic = true;  // ������ ����� ����

        // �߷��� 0���� ���� (��ų �߿���)
        rigid2D.gravityScale = 5;
    }

    void UnlockPosition()
    {
        isPositionLocked = false;
        rigid2D.isKinematic = false;  // ������ ����� �簳

        // �߷��� ������� ����
        rigid2D.gravityScale = 5;  // �⺻ �߷� ������ ���� (������ ����)
    }

    private float fixedYPosition;  // ��� �� ������ Y��ǥ ����

    void Dash()
    {
        if (canDash)
        {
            isDash = true;
            isInvincible = true;  // ��� �� ���� ����
            canDash = false;
            animator.SetBool("dashing", true);

            // ��� ���� �� Y��ǥ ����
            fixedYPosition = transform.position.y;

            float dashDirection = transform.localScale.x;  // ĳ���Ͱ� ���� �ִ� �������� ���
            rigid2D.velocity = new Vector2(dashDirection * dashDistance, 0);  // Y�� �ӵ� 0���� ����

            // ��� ���� Y��ǥ ����
            InvokeRepeating("LockYPosition", 0f, Time.fixedDeltaTime);

            Invoke("EndDash", 0.2f);  // 0.2�� �� ��� ����
            Invoke("ResetDash", dashCooldown);  // ��� ��Ÿ�� ����
        }
    }

    void LockYPosition()
    {
        // ������ Y��ǥ ����
        transform.position = new Vector3(transform.position.x, fixedYPosition, transform.position.z);
    }

    void EndDash()
    {
        // ��� ���� �� ��� ���� ����
        CancelInvoke("LockYPosition");  // Y��ǥ ���� ����
        rigid2D.velocity = Vector2.zero;  // ��� �� �ӵ� �ʱ�ȭ
        isDash = false;
        isInvincible = false;
        animator.SetBool("dashing", false);
    }



    void ResetDash()
    {
        canDash = true;  // ��� ���� ���·� ����
    }

    void Respawn()
    {
        // ������ �� �ʱ� ��ġ�� �̵�
        transform.position = respawnPosition;
        rigid2D.velocity = Vector2.zero;

        // ü�� ���� ó��
        nowHp -= 10;
        if (nowHp < 0)
        {
            nowHp = 0;

        }

        // ü�� �� ������Ʈ
        nowHpbar.fillAmount = (float)nowHp / (float)maxHp;
    }

    void SpawnEffect(GameObject effectPrefab)
    {
        if (effectPrefab != null)
        {
            // ����Ʈ ���� ��ġ ��� (ĳ������ ��ġ���� ������)
            Vector3 spawnPosition = transform.position;

            // ����Ʈ ����
            GameObject effect = Instantiate(effectPrefab, spawnPosition, Quaternion.identity);

            // ����Ʈ�� ���� ���� (�÷��̾ ������ ���� ����Ʈ�� ������)
            Vector3 effectScale = effect.transform.localScale;
            effectScale.x = Mathf.Abs(effectScale.x) * (transform.localScale.x > 0 ? 1 : -1);

            effect.transform.localScale = effectScale;
        }
    }

    void SpawnSwiftBladeEffect()
    {
        if (SwiftbladeEffectPrefab != null)
        {
            // ����Ʈ ���� ��ġ ��� (ĳ������ ��ġ���� ������)
            Vector3 spawnPosition = transform.position + new Vector3(transform.localScale.x * 1.5f, 0, 0);

            // ����Ʈ ����
            GameObject effect = Instantiate(SwiftbladeEffectPrefab, spawnPosition, Quaternion.identity);

            // ����Ʈ�� ���� ���� (�÷��̾ ������ ���� ����Ʈ�� ������)
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
            // ����Ʈ ���� ��ġ ��� (ĳ������ ��ġ���� ������)
            Vector3 spawnPosition1 = transform.position + new Vector3(transform.localScale.x * 1.0f, 0, 0);
            Vector3 spawnPosition2 = transform.position + new Vector3(transform.localScale.x * 1.0f, 0, 0);
            Vector3 spawnPosition3 = transform.position + new Vector3(transform.localScale.x * 1.0f, 0, 0);

            // ����Ʈ ����
            GameObject effect1 = Instantiate(WaterEffectPrefab, spawnPosition1, Quaternion.identity);
            GameObject effect2 = Instantiate(WaterEffectPrefab, spawnPosition1, Quaternion.identity);
            GameObject effect3 = Instantiate(WaterEffectPrefab, spawnPosition1, Quaternion.identity);

            // ����Ʈ�� ���� ���� (�÷��̾ ������ ���� ����Ʈ�� ������)
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

            // HomingMissile ������Ʈ�� �ִ� ��� �ʱ�ȭ
            HomingMissile homingMissile1 = effect1.GetComponent<HomingMissile>();
            HomingMissile homingMissile2 = effect2.GetComponent<HomingMissile>();
            HomingMissile homingMissile3 = effect3.GetComponent<HomingMissile>();
        }
    }

    void SpawnSwordAuraEffect()
    {
        if (SwordAuraEffectPrefab != null)
        {
            // ����Ʈ ���� ��ġ ��� (ĳ������ ��ġ���� ������)
            Vector3 spawnPosition = transform.position + new Vector3(transform.localScale.x * 1.0f, 0, 0);

            // ����Ʈ ����
            GameObject effect = Instantiate(SwordAuraEffectPrefab, spawnPosition, Quaternion.identity);

            // ����Ʈ�� ���� ���� (�÷��̾ ������ ���� ����Ʈ�� ������)
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
            // ����Ʈ ���� ��ġ ��� (ĳ������ ��ġ���� ������)
            Vector3 spawnPosition = transform.position + new Vector3(transform.localScale.x * 1.0f, 0, 0);

            // ����Ʈ ����
            GameObject effect = Instantiate(projectileSwordAuraEffectPrefab, spawnPosition, Quaternion.identity);

            // ����Ʈ�� ���� ���� (�÷��̾ ������ ���� ����Ʈ�� ������)
            Vector3 effectScale = effect.transform.localScale;
            effectScale.x = Mathf.Abs(effectScale.x) * (transform.localScale.x > 0 ? 1 : -1);

            effect.transform.localScale = effectScale;
        }
    }

    void SpawnChargeEffect()
    {
        if (ChargeEffectPrefab != null)
        {
            // ����Ʈ ���� ��ġ ��� (ĳ������ ��ġ���� ������)
            Vector3 spawnPosition = transform.position;

            // ����Ʈ ����
            GameObject effect = Instantiate(ChargeEffectPrefab, spawnPosition, Quaternion.identity);

            // ����Ʈ�� ���� ���� (�÷��̾ ������ ���� ����Ʈ�� ������)
            Vector3 effectScale = effect.transform.localScale;
            effectScale.x = Mathf.Abs(effectScale.x) * (transform.localScale.x > 0 ? 1 : -1);

            effect.transform.localScale = effectScale;
        }
    }

    void SpawnShieldReadyEffect()
    {
        if (ShieldReadyEffectPrefab != null)
        {
            // ����Ʈ ���� ��ġ ��� (ĳ������ ��ġ���� ������)
            Vector3 spawnPosition = transform.position;

            // ����Ʈ ����
            GameObject effect = Instantiate(ShieldReadyEffectPrefab, spawnPosition, Quaternion.identity);

            // ����Ʈ�� ���� ���� (�÷��̾ ������ ���� ����Ʈ�� ������)
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
            // ����Ʈ ���� ��ġ ��� (ĳ������ ��ġ����)
            Vector3 spawnPosition = transform.position;

            // ����Ʈ ����
            GameObject effect = Instantiate(ShieldEffectPrefab, spawnPosition, Quaternion.identity);

            // ����Ʈ�� ���� ���� (�÷��̾ ������ ���� ����Ʈ�� ������)
            Vector3 effectScale = effect.transform.localScale;
            effectScale.x = Mathf.Abs(effectScale.x) * (transform.localScale.x > 0 ? 1 : -1);

            effect.transform.localScale = effectScale;

            // �÷��̾ �θ�� �����Ͽ� ����Ʈ�� ����ٴϵ��� ����
            effect.transform.SetParent(transform);
        }
    }

    void SpawnFlameEffect()
    {
        if (FlameEffectPrefab != null)
        {
            // ����Ʈ ���� ��ġ ��� (ĳ������ ��ġ����)
            Vector3 spawnPosition = transform.position;

            // ����Ʈ ����
            GameObject effect = Instantiate(FlameEffectPrefab, spawnPosition, Quaternion.identity);

            // ����Ʈ�� ���� ���� (�÷��̾ ������ ���� ����Ʈ�� ������)
            Vector3 effectScale = effect.transform.localScale;
            effectScale.x = Mathf.Abs(effectScale.x) * (transform.localScale.x > 0 ? 1 : -1);

            effect.transform.localScale = effectScale;

            // �÷��̾ �θ�� �����Ͽ� ����Ʈ�� ����ٴϵ��� ����
            effect.transform.SetParent(transform);
        }
    }

    void SpawnNatureEffect()
    {
        if (NatureEffectPrefab != null)
        {
            // ����Ʈ ���� ��ġ ��� (ĳ������ ��ġ����)
            Vector3 spawnPosition = transform.position + new Vector3(transform.localScale.x * -0.1f, transform.localScale.y * 1.3f, 0);

            // ����Ʈ ����
            GameObject effect = Instantiate(NatureEffectPrefab, spawnPosition, Quaternion.identity);

            // ����Ʈ�� ���� ���� (�÷��̾ ������ ���� ����Ʈ�� ������)
            Vector3 effectScale = effect.transform.localScale;
            effectScale.x = Mathf.Abs(effectScale.x) * (transform.localScale.x > 0 ? 1 : -1);

            effect.transform.localScale = effectScale;

            // �÷��̾ �θ�� �����Ͽ� ����Ʈ�� ����ٴϵ��� ����
            effect.transform.SetParent(transform);
        }
    }

    void SpawnWindEffect()
    {
        if (WindEffectPrefab != null)
        {
            // ����Ʈ ���� ��ġ ��� (ĳ������ ��ġ����)
            Vector3 spawnPosition = transform.position;

            // ����Ʈ ����
            GameObject effect = Instantiate(WindEffectPrefab, spawnPosition, Quaternion.identity);

            // ����Ʈ�� ���� ���� (�÷��̾ ������ ���� ����Ʈ�� ������)
            Vector3 effectScale = effect.transform.localScale;
            effectScale.x = Mathf.Abs(effectScale.x) * (transform.localScale.x > 0 ? 1 : -1);

            effect.transform.localScale = effectScale;

            // �÷��̾ �θ�� �����Ͽ� ����Ʈ�� ����ٴϵ��� ����
            effect.transform.SetParent(transform);
        }
    }

    public void TakeDamage(int damage, Transform attacker)
    {
        // ���� �ִϸ��̼� ���¸� �����ɴϴ�.
        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);

        // Ư�� �ִϸ��̼��� ��� ������ Ȯ���մϴ�.
        if (currentState.IsName("elf_swiftBlade") || currentState.IsName("elf_water")    ||
            currentState.IsName("elf_sword_aura") || currentState.IsName("elf_charge")   ||
            currentState.IsName("elf_shield")     || currentState.IsName("elf_nature")   ||
            currentState.IsName("elf_flame")      || currentState.IsName("elf_air")     ||
            currentState.IsName("comboAttack1")   || currentState.IsName("comboAttack2") ||
            currentState.IsName("comboAttack3")   || currentState.IsName("comboAttack4") ||
            currentState.IsName("elf_dash")       || currentState.IsName("elf_sit"))
        {
            // Ư�� �ִϸ��̼��� ���� ���� ��� �������� ���� �� ������ �մϴ�.
            return;
        }

        if (!isHit)  // �̹� �ǰ� ���°� �ƴϸ�
        {
            nowHp -= damage;  // ü�� ����
            if (nowHp < 0)
            {
                nowHp = 0;  // ü�� �ּ�ȭ
            }

            nowHpbar.fillAmount = (float)nowHp / (float)maxHp;  // ü�¹� ������Ʈ

            // hit �ִϸ��̼� ���
            animator.SetTrigger("hit");
            isHit = true;

            // �˹� ó��
            // KnockBack(attacker);

            // ���� �ð� �� �ٽ� �ǰ� ���� ���·� ����
            Invoke("ResetHitState", 1.0f);  // ���÷� 1�� ���� �ǰ� �Ұ�
        }
    }

    public void natureHeal()
    {
        nowHp = nowHp += 20;
    }

    void KnockBack(Transform attacker)
    {
        float knockBackForce = 5.0f;  // �˹� ���� �ٿ��� �������� �̵� ����
        float knockBackDuration = 0.05f; // �˹� ���� �ð�

        // �˹� ���� ���� (���� ���ʿ� ������ ����������, �����ʿ� ������ ��������)
        float knockBackDirection = transform.position.x - attacker.position.x > 0 ? 1 : -1;

        // �˹� ���� (X�����θ� �о��)
        rigid2D.velocity = new Vector2(knockBackDirection * knockBackForce, rigid2D.velocity.y);

        // ���� �ð� �� �ӵ� �ʱ�ȭ (�˹��� ���� ��)
        Invoke("ResetVelocity", knockBackDuration);
    }

    void ResetVelocity()
    {
        rigid2D.velocity = new Vector2(0, rigid2D.velocity.y);  // X�� �ӵ��� 0���� �ʱ�ȭ
    }

    void ResetHitState()
    {
        isHit = false;  // �ǰ� ���� ���·� ����
    }

    void SetAttackSpeed(float speed)
    {
        animator.SetFloat("attackSpeed", speed);
        atkSpeed = speed;
    }

    public void Die()
    {
        animator.SetTrigger("die");            // die �ִϸ��̼� ����
        Destroy(gameObject, 3);                // 3���� ����
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
        // ���� �Ÿ��� ������ ������ �г� ǥ��
        if (traveledDistance >= distanceToLevelUp)
        {
            LevelUp();
            playerLevel++;
            startPosition = transform.position;  // �Ÿ� �ʱ�ȭ (�߰� ������ �����ϰ�)
        }
    }

    void LevelUp()
    {
        // ������ �г� ǥ�� (LevelUpUI ��ũ��Ʈ�� ShowLevelUpPanel �Լ� ȣ��)
        LevelUpUI levelUpUI = FindObjectOfType<LevelUpUI>();
        if (levelUpUI != null)
        {
            levelUpUI.ShowLevelUpPanel(playerLevel);
            Debug.Log("������! �̵� �Ÿ��� �޼��߽��ϴ�.");
        }
        else
        {
            Debug.LogError("LevelUpUI�� ã�� �� �����ϴ�.");
        }
    }

    void LoadEffectPrefab()
    {
        // SwiftbladeEffectPrefab�� Resources �������� �ε�
        SwiftbladeEffectPrefab = Resources.Load<GameObject>("Elf/Skill_Effect/Elf_Mech_Swiftblade_Effect");
        if (SwiftbladeEffectPrefab == null) Debug.LogError("SwiftbladeEffect �������� ã�� �� �����ϴ�! ��θ� Ȯ���ϼ���.");
        else if (SwiftbladeEffectPrefab) Debug.Log("SwiftbladeEffect ������ �ε� �Ϸ�.");

        // SwiftBladeffectPrefab�� Resources �������� �ε�
        WaterEffectPrefab = Resources.Load<GameObject>("Elf/Skill_Effect/Projectile Test");
        if (WaterEffectPrefab == null) Debug.LogError("WaterEffect �������� ã�� �� �����ϴ�! ��θ� Ȯ���ϼ���.");
        else if (WaterEffectPrefab) Debug.Log("WaterEffect ������ �ε� �Ϸ�.");

        // SwordAuraEffectPrefab�� Resources �������� �ε�
        SwordAuraEffectPrefab = Resources.Load<GameObject>("Elf/Skill_Effect/Elf_Mech_SwordAura_Effect");
        if (SwordAuraEffectPrefab == null) Debug.LogError("SwordAuraEffect �������� ã�� �� �����ϴ�! ��θ� Ȯ���ϼ���.");
        else if (SwordAuraEffectPrefab) Debug.Log("SwordAuraEffect ������ �ε� �Ϸ�.");

        // projectileSwordAuraEffectPrefab�� Resources �������� �ε�
        projectileSwordAuraEffectPrefab = Resources.Load<GameObject>("Elf/Skill_Effect/Projectile_Mech_SwordAura");
        if (projectileSwordAuraEffectPrefab == null) Debug.LogError("projectileSwordAuraEffect �������� ã�� �� �����ϴ�! ��θ� Ȯ���ϼ���.");
        else if (projectileSwordAuraEffectPrefab) Debug.Log("projectileSwordAuraEffect ������ �ε� �Ϸ�.");

        // ChargeEffectPrefab�� Resources �������� �ε�
        ChargeEffectPrefab = Resources.Load<GameObject>("Elf/Skill_Effect/Elf_Mech_Charge_Effect");
        if (ChargeEffectPrefab == null) Debug.LogError("ChargeEffect �������� ã�� �� �����ϴ�! ��θ� Ȯ���ϼ���.");
        else if (ChargeEffectPrefab) Debug.Log("ChargeEffect ������ �ε� �Ϸ�.");

        // ShieldReadyEffectPrefab�� Resources �������� �ε�
        ShieldReadyEffectPrefab = Resources.Load<GameObject>("Elf/Skill_Effect/Elf_Mech_Shield_Ready_Effect");
        if (ShieldReadyEffectPrefab == null) Debug.LogError("ShieldReadyEffect �������� ã�� �� �����ϴ�! ��θ� Ȯ���ϼ���.");
        else if (ShieldReadyEffectPrefab) Debug.Log("ShieldReadyEffect ������ �ε� �Ϸ�.");

        // ShieldEffectPrefab�� Resources �������� �ε�
        ShieldEffectPrefab = Resources.Load<GameObject>("Elf/Skill_Effect/Elf_Mech_Shield_Effect");
        if (ShieldEffectPrefab == null) Debug.LogError("ShieldEffect �������� ã�� �� �����ϴ�! ��θ� Ȯ���ϼ���.");
        else if (ShieldEffectPrefab) Debug.Log("ShieldEffect ������ �ε� �Ϸ�.");

        // FlameEffectPrefab�� Resources �������� �ε�
        FlameEffectPrefab = Resources.Load<GameObject>("Elf/Skill_Effect/Elf_Magic_Flame_Effect");
        if (FlameEffectPrefab == null) Debug.LogError("FlameEffect �������� ã�� �� �����ϴ�! ��θ� Ȯ���ϼ���.");
        else if (FlameEffectPrefab) Debug.Log("FlameEffect ������ �ε� �Ϸ�.");

        // NatureEffectPrefab�� Resources �������� �ε�
        NatureEffectPrefab = Resources.Load<GameObject>("Elf/Skill_Effect/Elf_Tree_Nature_Effect");
        if (NatureEffectPrefab == null) Debug.LogError("NatureEffect �������� ã�� �� �����ϴ�! ��θ� Ȯ���ϼ���.");
        else if (NatureEffectPrefab) Debug.Log("NatureEffect ������ �ε� �Ϸ�.");

        // WindEffectPrefab�� Resources �������� �ε�
        WindEffectPrefab = Resources.Load<GameObject>("Elf/Skill_Effect/Elf_Magic_Wind");
        if (WindEffectPrefab == null) Debug.LogError("WindEffect �������� ã�� �� �����ϴ�! ��θ� Ȯ���ϼ���.");
        else if (WindEffectPrefab) Debug.Log("WindEffect ������ �ε� �Ϸ�.");
    }

    void chargeMoveDealy() 
    {
        // ���� �Ÿ���ŭ �̵� (�Ÿ�: 5f)
        float moveDistance = 3f;
        Vector3 moveDirection = transform.right * (transform.localScale.x > 0 ? 1 : -1); // ���� �ٶ󺸴� ����(���� x ���̸� ������, ���� x ���̸� ����)
        transform.position += moveDirection * moveDistance;
    }
}
