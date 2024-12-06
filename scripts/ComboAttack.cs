using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ComboAttack : MonoBehaviour
{
    // �޺� �Է��� ��ȿ�� �ð� (�� ����)
    public float comboResetTime = 0.4f;

    // ���� �޺� �ܰ� (0�� �޺��� ����)
    private int currentComboStep = 0;

    // �޺� �Է� ������ �ð��� �����ϱ� ���� Ÿ�̸�
    private float comboTimer = 0.5f;

    // Animator ������Ʈ (ĳ������ �ִϸ��̼� ����)
    private Animator animator;
    public Elf elf;

    public GameObject attack1EffectPrefab;  // ù ��° ���� ����Ʈ ������
    public GameObject attack2EffectPrefab;  // �� ��° ���� ����Ʈ ������
    public GameObject attack3EffectPrefab;  // �� ��° ���� ����Ʈ ������
    public GameObject attack4EffectPrefab;  // �� ��° ���� ����Ʈ ������

    // �ʱ�ȭ �� Animator ������Ʈ ��������
    void Start()
    {
        animator = GetComponent<Animator>();
        elf = GetComponent<Elf>();

        attackEffectPrefabCheck();  
    }

    // �� �����Ӹ��� �޺� �Է��� Ȯ���ϰ� Ÿ�̸Ӹ� ������Ʈ
    void Update()
    {
        // ���� �Է��� ���Դ��� Ȯ�� (���콺 ��Ŭ��)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // �޺� �Է� ó��
            HandleComboInput();
        }

        // ���� �޺� �ܰ谡 0���� ũ��(�޺� ���̸�) Ÿ�̸Ӹ� ������Ʈ
        if (currentComboStep > 0)
        {
            // Ÿ�̸Ӹ� ��Ÿ �ð���ŭ ������Ŵ
            comboTimer += Time.deltaTime;

            // ���� Ÿ�̸Ӱ� ��ȿ �ð��� �ʰ��ϸ� �޺��� �ʱ�ȭ
            if (comboTimer > comboResetTime)
            {
                ResetCombo();
            }
        }
    }

    // �޺� �Է��� ó���ϴ� �Լ�
    void HandleComboInput()
    {
        if (currentComboStep == 0 && !animator.GetCurrentAnimatorStateInfo(0).IsName("comboAttack1"))
        {
            Debug.Log("comboResetTime : " + comboResetTime);
            comboResetTime = 0.4f;
            currentComboStep = 1;
            Debug.Log("�޺� : " + currentComboStep);
            animator.SetInteger("comboCount", 1);
            SpawnAttack1Effect();
            comboTimer = 0f;
        }

        else if (currentComboStep == 1 && comboTimer < comboResetTime && !animator.GetCurrentAnimatorStateInfo(0).IsName("comboAttack2"))
        {
            currentComboStep = 2;
            Debug.Log("�޺� : " + currentComboStep);
            animator.SetInteger("comboCount", 2);
            SpawnAttack2Effect();
            comboTimer = 0f;
        }

        else if (currentComboStep == 2 && comboTimer < comboResetTime && !animator.GetCurrentAnimatorStateInfo(0).IsName("comboAttack3"))
        {
            currentComboStep = 3;
            Debug.Log("�޺� : " + currentComboStep);
            animator.SetInteger("comboCount", 3);
            SpawnAttack3Effect();
            comboTimer = 0f;
        }

        else if (currentComboStep == 3 && comboTimer < comboResetTime && !animator.GetCurrentAnimatorStateInfo(0).IsName("comboAttack4"))
        {
            currentComboStep = 4;
            Debug.Log("�޺� : " + currentComboStep);
            animator.SetInteger("comboCount", 4);
            SpawnAttack4Effect();
            comboTimer = 0f;
        }

        else if (currentComboStep == 4) 
        {
            comboResetTime = 0f;
            Debug.Log("comboResetTime : " + comboResetTime);
        }
    }

    // �޺��� �ʱ�ȭ�ϴ� �Լ�
    void ResetCombo()
    {
        currentComboStep = 0;    // �޺� �ܰ踦 �ʱ� ���·� ����
        animator.SetInteger("comboCount", 0);
        comboTimer = 0f;         // Ÿ�̸ӵ� �ʱ�ȭ
        Debug.Log("�޺��� �ʱ�ȭ �Ǿ����ϴ�. ���� �޺� ī��Ʈ�� " + currentComboStep + "�Դϴ�.");
    }

    private void attackEffectPrefabCheck()
    {
        // attackEffectPrefab�� Resources �������� �ε�
        attack1EffectPrefab = Resources.Load<GameObject>("Elf/Skill_Effect/Elf_Attack1_Effect");
        attack2EffectPrefab = Resources.Load<GameObject>("Elf/Skill_Effect/Elf_Attack2_Effect");
        attack3EffectPrefab = Resources.Load<GameObject>("Elf/Skill_Effect/Elf_Attack3_Effect");
        attack4EffectPrefab = Resources.Load<GameObject>("Elf/Skill_Effect/Elf_Attack4_Effect");

        if (attack1EffectPrefab == null) Debug.LogError("Attack_1_Effect �������� ã�� �� �����ϴ�! ��θ� Ȯ���ϼ���.");
        else if (attack1EffectPrefab) Debug.Log("Attack_1_Effect ������ �ε� �Ϸ�.");

        if (attack2EffectPrefab == null) Debug.LogError("Attack_2_Effect �������� ã�� �� �����ϴ�! ��θ� Ȯ���ϼ���.");
        else if (attack2EffectPrefab) Debug.Log("Attack_2_Effect ������ �ε� �Ϸ�.");

        if (attack3EffectPrefab == null) Debug.LogError("Attack_3_Effect �������� ã�� �� �����ϴ�! ��θ� Ȯ���ϼ���.");
        else if (attack3EffectPrefab) Debug.Log("Attack_3_Effect ������ �ε� �Ϸ�.");

        if (attack4EffectPrefab == null) Debug.LogError("Attack_4_Effect �������� ã�� �� �����ϴ�! ��θ� Ȯ���ϼ���.");
        else if (attack4EffectPrefab) Debug.Log("Attack_4_Effect ������ �ε� �Ϸ�.");
    }

    void SpawnAttack1Effect()
    {
        if (attack1EffectPrefab != null)
        {
            // ����Ʈ ���� ��ġ ��� (ĳ������ ��ġ���� ������)
            Vector3 spawnPosition = transform.position + new Vector3(transform.localScale.x * 1.0f, 0, 0);

            // ����Ʈ ����
            GameObject effect = Instantiate(attack1EffectPrefab, spawnPosition, Quaternion.identity);

            // ����Ʈ�� ���� ���� (�÷��̾ ������ ���� ����Ʈ�� ������)
            Vector3 effectScale = effect.transform.localScale;
            effectScale.x = Mathf.Abs(effectScale.x) * (transform.localScale.x > 0 ? 1 : -1);

            effect.transform.localScale = effectScale;
        }
        elf.attacked = false; // ���� �� attacked�� false�� ����
    }

    void SpawnAttack2Effect()
    {
        if (attack2EffectPrefab != null)
        {
            // ����Ʈ ���� ��ġ ��� (ĳ������ ��ġ���� ������)
            Vector3 spawnPosition = transform.position + new Vector3(transform.localScale.x * 1.0f, 0, 0);

            // ����Ʈ ����
            GameObject effect = Instantiate(attack2EffectPrefab, spawnPosition, Quaternion.identity);

            // ����Ʈ�� ���� ���� (�÷��̾ ������ ���� ����Ʈ�� ������)
            Vector3 effectScale = effect.transform.localScale;
            effectScale.x = Mathf.Abs(effectScale.x) * (transform.localScale.x > 0 ? 1 : -1);

            effect.transform.localScale = effectScale;
        }
        elf.attacked = false; // ���� �� attacked�� false�� ����
    }

    void SpawnAttack3Effect()
    {
        if (attack3EffectPrefab != null)
        {
            // ����Ʈ ���� ��ġ ��� (ĳ������ ��ġ���� ������)
            Vector3 spawnPosition = transform.position + new Vector3(transform.localScale.x * 1.0f, 0, 0);

            // ����Ʈ ����
            GameObject effect = Instantiate(attack3EffectPrefab, spawnPosition, Quaternion.identity);

            // ����Ʈ�� ���� ���� (�÷��̾ ������ ���� ����Ʈ�� ������)
            Vector3 effectScale = effect.transform.localScale;
            effectScale.x = Mathf.Abs(effectScale.x) * (transform.localScale.x > 0 ? 1 : -1);

            effect.transform.localScale = effectScale;
        }
        elf.attacked = false; // ���� �� attacked�� false�� ����
    }

    void SpawnAttack4Effect()
    {
        if (attack4EffectPrefab != null)
        {
            Vector3 spawnPosition = transform.position + new Vector3(transform.localScale.x * 1.0f, 0, 0);
            GameObject effect = Instantiate(attack4EffectPrefab, spawnPosition, Quaternion.identity);
            Vector3 effectScale = effect.transform.localScale;
            effectScale.x = Mathf.Abs(effectScale.x) * (transform.localScale.x > 0 ? 1 : -1);
            effect.transform.localScale = effectScale;
        }
        elf.attacked = false;
    }
}
