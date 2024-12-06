using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ComboAttack : MonoBehaviour
{
    // 콤보 입력이 유효한 시간 (초 단위)
    public float comboResetTime = 0.4f;

    // 현재 콤보 단계 (0은 콤보가 없음)
    private int currentComboStep = 0;

    // 콤보 입력 사이의 시간을 추적하기 위한 타이머
    private float comboTimer = 0.5f;

    // Animator 컴포넌트 (캐릭터의 애니메이션 제어)
    private Animator animator;
    public Elf elf;

    public GameObject attack1EffectPrefab;  // 첫 번째 공격 이펙트 프리팹
    public GameObject attack2EffectPrefab;  // 두 번째 공격 이펙트 프리팹
    public GameObject attack3EffectPrefab;  // 세 번째 공격 이펙트 프리팹
    public GameObject attack4EffectPrefab;  // 네 번째 공격 이펙트 프리팹

    // 초기화 시 Animator 컴포넌트 가져오기
    void Start()
    {
        animator = GetComponent<Animator>();
        elf = GetComponent<Elf>();

        attackEffectPrefabCheck();  
    }

    // 매 프레임마다 콤보 입력을 확인하고 타이머를 업데이트
    void Update()
    {
        // 공격 입력이 들어왔는지 확인 (마우스 좌클릭)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // 콤보 입력 처리
            HandleComboInput();
        }

        // 현재 콤보 단계가 0보다 크면(콤보 중이면) 타이머를 업데이트
        if (currentComboStep > 0)
        {
            // 타이머를 델타 시간만큼 증가시킴
            comboTimer += Time.deltaTime;

            // 만약 타이머가 유효 시간을 초과하면 콤보를 초기화
            if (comboTimer > comboResetTime)
            {
                ResetCombo();
            }
        }
    }

    // 콤보 입력을 처리하는 함수
    void HandleComboInput()
    {
        if (currentComboStep == 0 && !animator.GetCurrentAnimatorStateInfo(0).IsName("comboAttack1"))
        {
            Debug.Log("comboResetTime : " + comboResetTime);
            comboResetTime = 0.4f;
            currentComboStep = 1;
            Debug.Log("콤보 : " + currentComboStep);
            animator.SetInteger("comboCount", 1);
            SpawnAttack1Effect();
            comboTimer = 0f;
        }

        else if (currentComboStep == 1 && comboTimer < comboResetTime && !animator.GetCurrentAnimatorStateInfo(0).IsName("comboAttack2"))
        {
            currentComboStep = 2;
            Debug.Log("콤보 : " + currentComboStep);
            animator.SetInteger("comboCount", 2);
            SpawnAttack2Effect();
            comboTimer = 0f;
        }

        else if (currentComboStep == 2 && comboTimer < comboResetTime && !animator.GetCurrentAnimatorStateInfo(0).IsName("comboAttack3"))
        {
            currentComboStep = 3;
            Debug.Log("콤보 : " + currentComboStep);
            animator.SetInteger("comboCount", 3);
            SpawnAttack3Effect();
            comboTimer = 0f;
        }

        else if (currentComboStep == 3 && comboTimer < comboResetTime && !animator.GetCurrentAnimatorStateInfo(0).IsName("comboAttack4"))
        {
            currentComboStep = 4;
            Debug.Log("콤보 : " + currentComboStep);
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

    // 콤보를 초기화하는 함수
    void ResetCombo()
    {
        currentComboStep = 0;    // 콤보 단계를 초기 상태로 리셋
        animator.SetInteger("comboCount", 0);
        comboTimer = 0f;         // 타이머도 초기화
        Debug.Log("콤보가 초기화 되었습니다. 지금 콤보 카운트는 " + currentComboStep + "입니다.");
    }

    private void attackEffectPrefabCheck()
    {
        // attackEffectPrefab을 Resources 폴더에서 로드
        attack1EffectPrefab = Resources.Load<GameObject>("Elf/Skill_Effect/Elf_Attack1_Effect");
        attack2EffectPrefab = Resources.Load<GameObject>("Elf/Skill_Effect/Elf_Attack2_Effect");
        attack3EffectPrefab = Resources.Load<GameObject>("Elf/Skill_Effect/Elf_Attack3_Effect");
        attack4EffectPrefab = Resources.Load<GameObject>("Elf/Skill_Effect/Elf_Attack4_Effect");

        if (attack1EffectPrefab == null) Debug.LogError("Attack_1_Effect 프리팹을 찾을 수 없습니다! 경로를 확인하세요.");
        else if (attack1EffectPrefab) Debug.Log("Attack_1_Effect 프리팹 로드 완료.");

        if (attack2EffectPrefab == null) Debug.LogError("Attack_2_Effect 프리팹을 찾을 수 없습니다! 경로를 확인하세요.");
        else if (attack2EffectPrefab) Debug.Log("Attack_2_Effect 프리팹 로드 완료.");

        if (attack3EffectPrefab == null) Debug.LogError("Attack_3_Effect 프리팹을 찾을 수 없습니다! 경로를 확인하세요.");
        else if (attack3EffectPrefab) Debug.Log("Attack_3_Effect 프리팹 로드 완료.");

        if (attack4EffectPrefab == null) Debug.LogError("Attack_4_Effect 프리팹을 찾을 수 없습니다! 경로를 확인하세요.");
        else if (attack4EffectPrefab) Debug.Log("Attack_4_Effect 프리팹 로드 완료.");
    }

    void SpawnAttack1Effect()
    {
        if (attack1EffectPrefab != null)
        {
            // 이펙트 생성 위치 계산 (캐릭터의 위치에서 앞으로)
            Vector3 spawnPosition = transform.position + new Vector3(transform.localScale.x * 1.0f, 0, 0);

            // 이펙트 생성
            GameObject effect = Instantiate(attack1EffectPrefab, spawnPosition, Quaternion.identity);

            // 이펙트의 방향 설정 (플레이어가 왼쪽을 보면 이펙트를 뒤집음)
            Vector3 effectScale = effect.transform.localScale;
            effectScale.x = Mathf.Abs(effectScale.x) * (transform.localScale.x > 0 ? 1 : -1);

            effect.transform.localScale = effectScale;
        }
        elf.attacked = false; // 공격 후 attacked를 false로 설정
    }

    void SpawnAttack2Effect()
    {
        if (attack2EffectPrefab != null)
        {
            // 이펙트 생성 위치 계산 (캐릭터의 위치에서 앞으로)
            Vector3 spawnPosition = transform.position + new Vector3(transform.localScale.x * 1.0f, 0, 0);

            // 이펙트 생성
            GameObject effect = Instantiate(attack2EffectPrefab, spawnPosition, Quaternion.identity);

            // 이펙트의 방향 설정 (플레이어가 왼쪽을 보면 이펙트를 뒤집음)
            Vector3 effectScale = effect.transform.localScale;
            effectScale.x = Mathf.Abs(effectScale.x) * (transform.localScale.x > 0 ? 1 : -1);

            effect.transform.localScale = effectScale;
        }
        elf.attacked = false; // 공격 후 attacked를 false로 설정
    }

    void SpawnAttack3Effect()
    {
        if (attack3EffectPrefab != null)
        {
            // 이펙트 생성 위치 계산 (캐릭터의 위치에서 앞으로)
            Vector3 spawnPosition = transform.position + new Vector3(transform.localScale.x * 1.0f, 0, 0);

            // 이펙트 생성
            GameObject effect = Instantiate(attack3EffectPrefab, spawnPosition, Quaternion.identity);

            // 이펙트의 방향 설정 (플레이어가 왼쪽을 보면 이펙트를 뒤집음)
            Vector3 effectScale = effect.transform.localScale;
            effectScale.x = Mathf.Abs(effectScale.x) * (transform.localScale.x > 0 ? 1 : -1);

            effect.transform.localScale = effectScale;
        }
        elf.attacked = false; // 공격 후 attacked를 false로 설정
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
