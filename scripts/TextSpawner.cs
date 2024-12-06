using UnityEngine;
using TMPro;
using System.Collections;

public class TextSpawner : MonoBehaviour
{
    public GameObject textPrefab; // TextMeshPro UI 프리팹
    public Transform canvasTransform; // UI를 부모로 할 Canvas Transform
    public Vector3 spawnPosition = Vector3.zero; // 생성 위치
    public Vector3 yOffset = new Vector3(0, 1, 0);

    public Elf elf; // elf 스크립트 참조
    public AttackEffect AtkEffect;

    void Start()
    {
        // 같은 GameObject에 붙어 있는 Elf 스크립트를 참조
        elf = FindObjectOfType<Elf>();

        // AtkEffect가 연결되었는지 확인
        // if (AtkEffect == null)
        // {
        //     Debug.LogWarning("초기 상태에서 AtkEffect가 연결되지 않았습니다.");
        // }
    }

    void Update()
    {

        Enemy enemy = FindObjectOfType<Enemy>(); // 게임에서 현재 Enemy를 찾음

        // AtkEffect가 null인 경우 현재 활성화된 AttackEffect를 다시 찾음
        if (AtkEffect == null) AtkEffect = FindObjectOfType<AttackEffect>();

        // AtkEffect가 Null이 아니고 tookDamage가 true인 경우에만 실행
        if (AtkEffect != null && AtkEffect.tookDamage)
        {
            SpawnText(enemy);
            Debug.Log("데미지 : " + elf.atkDmg);

            StartCoroutine(ResetTookDamageAfterDelay());
        }
    }

    private void OnDestroy()
    {
        if (AtkEffect != null)
        {
            AtkEffect.OnMonsterTookDamage -= HandleMonsterTookDamage;
        }
    }

    private void HandleMonsterTookDamage(int damage)
    {
        // 적의 데미지를 받았을 때 텍스트 표시
        Enemy enemy = FindObjectOfType<Enemy>(); // 게임에서 현재 Enemy를 찾음
        if (enemy != null)
        {
            Debug.Log("데미지 : " + damage);
            SpawnText(enemy); // enemy 객체 전달
        }
    }

    public void SpawnText(Enemy enemy) // 매개변수로 Enemy를 받도록 변경
    {
        if (textPrefab != null && canvasTransform != null)
        {
            // TextMeshPro UI 프리팹 생성
            GameObject newText = Instantiate(textPrefab, canvasTransform);

            // 몬스터의 위치에 텍스트를 표시
            Vector3 spawnPos = enemy.transform.position + yOffset; // 몬스터 위치 기준
            newText.transform.position = spawnPos;

            // 생성된 DamagePopup 설정
            DamagePopup damagePopup = newText.GetComponent<DamagePopup>();

            if (damagePopup != null)
            {
                damagePopup.damage = elf.atkDmg;  // 데미지 설정
            }
            else
            {
                Debug.LogWarning("DamagePopup 컴포넌트를 찾을 수 없습니다.");
            }

            // 2초 후 삭제
            StartCoroutine(DestroyAfterDelay(newText, 2f));
        }
        else
        {
            Debug.LogWarning("TextPrefab 또는 CanvasTransform이 연결되지 않았습니다.");
        }
    }

    public void SetAttackEffect(AttackEffect newEffect)
    {
        AtkEffect = newEffect;  // AttackEffect의 참조를 설정
    }

    private IEnumerator DestroyAfterDelay(GameObject obj, float delay)
    {
        // 지정된 시간 동안 대기
        yield return new WaitForSeconds(delay);

        // 오브젝트 제거
        Destroy(obj);
    }

    private IEnumerator ResetTookDamageAfterDelay()
    {
        yield return new WaitForSeconds(0.1f); // 잠시 대기
        Debug.Log("tookDamage를 리셋합니다.");
        AtkEffect.ResetTookDamage(); // tookDamage 리셋
    }
}
