using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class AttackEffect : MonoBehaviour
{
    public int damage; // 공격 이펙트가 줄 데미지
    public float lifetime = 0.2f;
    public bool tookDamage = false;
    public Elf elf;
    public EnemyAI enemyAI;

    private Collider2D other; // Collider2D를 멤버 변수로 추가

    public event Action<int> OnMonsterTookDamage;

    void Start()
    {
        // 공격 이펙트가 생성될 때, 기본적으로 Elf의 atkDmg로 데미지 설정
        elf = FindObjectOfType<Elf>();
        if (elf != null)
        {
            damage = elf.atkDmg;
        }
        else
        {
            Debug.LogWarning("Elf 인스턴스를 찾을 수 없습니다. 데미지가 초기화되지 않았습니다.");
        }

        // EnemyAI 인스턴스 초기화
        enemyAI = FindObjectOfType<EnemyAI>();
        if (enemyAI == null)
        {
            Debug.LogWarning("EnemyAI 인스턴스를 찾을 수 없습니다.");
        }

        // TextSpawner에 AttackEffect를 전달
        TextSpawner textSpawner = FindObjectOfType<TextSpawner>();
        if (textSpawner != null)
        {
            textSpawner.SetAttackEffect(this);  // TextSpawner에 AttackEffect를 전달
        }
        else
        {
            Debug.LogWarning("TextSpawner 인스턴스를 찾을 수 없습니다.");
        }

        Destroy(gameObject, lifetime); // lifetime 후에 자신을 파괴
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("충돌 감지: " + other.gameObject.name);

        this.other = other; // 충돌한 Collider2D를 저장

        if (gameObject.CompareTag("Enemy_Attack_Effect") && other.CompareTag("Player"))
        {
            // 적의 공격 이펙트가 플레이어에 충돌 시
            Elf player = other.GetComponent<Elf>();
            if (player != null && enemyAI != null)
            {
                enemyAI.DealDamageToTarget(); // enemyAI를 통해 플레이어에게 데미지 적용
                Debug.Log($"플레이어가 데미지를 받았습니다. 현재 HP: {player.nowHp}");
            }
            else if (player == null)
            {
                Debug.LogWarning("플레이어에 Elf 컴포넌트가 없습니다.");
            }
            else if (enemyAI == null)
            {
                Debug.LogWarning("EnemyAI가 초기화되지 않아 데미지를 적용할 수 없습니다.");
            }
        }
        else if (gameObject.CompareTag("AttackEffect") && other.CompareTag("Monster"))
        {
            // ApplyDamageToEnemy 호출 시, 파라미터를 넘기지 않음
            ApplyDamageToEnemy(); // 이제 파라미터 없이 호출

            ResetTookDamage();
        }

    }

    // ApplyDamageToEnemy 함수에서 'other'를 직접 처리하도록 변경
    private void ApplyDamageToEnemy()
    {
        if (other != null)
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.nowHp -= damage; // 데미지 감소 처리
                enemy.enemyAnimator?.SetTrigger("hit"); // 히트 애니메이션 트리거
                tookDamage = true;

                // TextSpawner에 몬스터 정보 전달
                TextSpawner textSpawner = FindObjectOfType<TextSpawner>();
                if (textSpawner != null)
                {
                    textSpawner.SpawnText(enemy); // 텍스트 표시
                }

                // 몬스터가 사망하는 경우 처리
                if (enemy.nowHp <= 0)
                {
                    enemy.Die();
                }
            }
        }
    }

    public void ResetTookDamage()
    {
        tookDamage = false;
    }
}
