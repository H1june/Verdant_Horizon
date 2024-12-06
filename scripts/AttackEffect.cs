using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class AttackEffect : MonoBehaviour
{
    public int damage; // ���� ����Ʈ�� �� ������
    public float lifetime = 0.2f;
    public bool tookDamage = false;
    public Elf elf;
    public EnemyAI enemyAI;

    private Collider2D other; // Collider2D�� ��� ������ �߰�

    public event Action<int> OnMonsterTookDamage;

    void Start()
    {
        // ���� ����Ʈ�� ������ ��, �⺻������ Elf�� atkDmg�� ������ ����
        elf = FindObjectOfType<Elf>();
        if (elf != null)
        {
            damage = elf.atkDmg;
        }
        else
        {
            Debug.LogWarning("Elf �ν��Ͻ��� ã�� �� �����ϴ�. �������� �ʱ�ȭ���� �ʾҽ��ϴ�.");
        }

        // EnemyAI �ν��Ͻ� �ʱ�ȭ
        enemyAI = FindObjectOfType<EnemyAI>();
        if (enemyAI == null)
        {
            Debug.LogWarning("EnemyAI �ν��Ͻ��� ã�� �� �����ϴ�.");
        }

        // TextSpawner�� AttackEffect�� ����
        TextSpawner textSpawner = FindObjectOfType<TextSpawner>();
        if (textSpawner != null)
        {
            textSpawner.SetAttackEffect(this);  // TextSpawner�� AttackEffect�� ����
        }
        else
        {
            Debug.LogWarning("TextSpawner �ν��Ͻ��� ã�� �� �����ϴ�.");
        }

        Destroy(gameObject, lifetime); // lifetime �Ŀ� �ڽ��� �ı�
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("�浹 ����: " + other.gameObject.name);

        this.other = other; // �浹�� Collider2D�� ����

        if (gameObject.CompareTag("Enemy_Attack_Effect") && other.CompareTag("Player"))
        {
            // ���� ���� ����Ʈ�� �÷��̾ �浹 ��
            Elf player = other.GetComponent<Elf>();
            if (player != null && enemyAI != null)
            {
                enemyAI.DealDamageToTarget(); // enemyAI�� ���� �÷��̾�� ������ ����
                Debug.Log($"�÷��̾ �������� �޾ҽ��ϴ�. ���� HP: {player.nowHp}");
            }
            else if (player == null)
            {
                Debug.LogWarning("�÷��̾ Elf ������Ʈ�� �����ϴ�.");
            }
            else if (enemyAI == null)
            {
                Debug.LogWarning("EnemyAI�� �ʱ�ȭ���� �ʾ� �������� ������ �� �����ϴ�.");
            }
        }
        else if (gameObject.CompareTag("AttackEffect") && other.CompareTag("Monster"))
        {
            // ApplyDamageToEnemy ȣ�� ��, �Ķ���͸� �ѱ��� ����
            ApplyDamageToEnemy(); // ���� �Ķ���� ���� ȣ��

            ResetTookDamage();
        }

    }

    // ApplyDamageToEnemy �Լ����� 'other'�� ���� ó���ϵ��� ����
    private void ApplyDamageToEnemy()
    {
        if (other != null)
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.nowHp -= damage; // ������ ���� ó��
                enemy.enemyAnimator?.SetTrigger("hit"); // ��Ʈ �ִϸ��̼� Ʈ����
                tookDamage = true;

                // TextSpawner�� ���� ���� ����
                TextSpawner textSpawner = FindObjectOfType<TextSpawner>();
                if (textSpawner != null)
                {
                    textSpawner.SpawnText(enemy); // �ؽ�Ʈ ǥ��
                }

                // ���Ͱ� ����ϴ� ��� ó��
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
