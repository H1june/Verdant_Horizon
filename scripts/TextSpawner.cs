using UnityEngine;
using TMPro;
using System.Collections;

public class TextSpawner : MonoBehaviour
{
    public GameObject textPrefab; // TextMeshPro UI ������
    public Transform canvasTransform; // UI�� �θ�� �� Canvas Transform
    public Vector3 spawnPosition = Vector3.zero; // ���� ��ġ
    public Vector3 yOffset = new Vector3(0, 1, 0);

    public Elf elf; // elf ��ũ��Ʈ ����
    public AttackEffect AtkEffect;

    void Start()
    {
        // ���� GameObject�� �پ� �ִ� Elf ��ũ��Ʈ�� ����
        elf = FindObjectOfType<Elf>();

        // AtkEffect�� ����Ǿ����� Ȯ��
        // if (AtkEffect == null)
        // {
        //     Debug.LogWarning("�ʱ� ���¿��� AtkEffect�� ������� �ʾҽ��ϴ�.");
        // }
    }

    void Update()
    {

        Enemy enemy = FindObjectOfType<Enemy>(); // ���ӿ��� ���� Enemy�� ã��

        // AtkEffect�� null�� ��� ���� Ȱ��ȭ�� AttackEffect�� �ٽ� ã��
        if (AtkEffect == null) AtkEffect = FindObjectOfType<AttackEffect>();

        // AtkEffect�� Null�� �ƴϰ� tookDamage�� true�� ��쿡�� ����
        if (AtkEffect != null && AtkEffect.tookDamage)
        {
            SpawnText(enemy);
            Debug.Log("������ : " + elf.atkDmg);

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
        // ���� �������� �޾��� �� �ؽ�Ʈ ǥ��
        Enemy enemy = FindObjectOfType<Enemy>(); // ���ӿ��� ���� Enemy�� ã��
        if (enemy != null)
        {
            Debug.Log("������ : " + damage);
            SpawnText(enemy); // enemy ��ü ����
        }
    }

    public void SpawnText(Enemy enemy) // �Ű������� Enemy�� �޵��� ����
    {
        if (textPrefab != null && canvasTransform != null)
        {
            // TextMeshPro UI ������ ����
            GameObject newText = Instantiate(textPrefab, canvasTransform);

            // ������ ��ġ�� �ؽ�Ʈ�� ǥ��
            Vector3 spawnPos = enemy.transform.position + yOffset; // ���� ��ġ ����
            newText.transform.position = spawnPos;

            // ������ DamagePopup ����
            DamagePopup damagePopup = newText.GetComponent<DamagePopup>();

            if (damagePopup != null)
            {
                damagePopup.damage = elf.atkDmg;  // ������ ����
            }
            else
            {
                Debug.LogWarning("DamagePopup ������Ʈ�� ã�� �� �����ϴ�.");
            }

            // 2�� �� ����
            StartCoroutine(DestroyAfterDelay(newText, 2f));
        }
        else
        {
            Debug.LogWarning("TextPrefab �Ǵ� CanvasTransform�� ������� �ʾҽ��ϴ�.");
        }
    }

    public void SetAttackEffect(AttackEffect newEffect)
    {
        AtkEffect = newEffect;  // AttackEffect�� ������ ����
    }

    private IEnumerator DestroyAfterDelay(GameObject obj, float delay)
    {
        // ������ �ð� ���� ���
        yield return new WaitForSeconds(delay);

        // ������Ʈ ����
        Destroy(obj);
    }

    private IEnumerator ResetTookDamageAfterDelay()
    {
        yield return new WaitForSeconds(0.1f); // ��� ���
        Debug.Log("tookDamage�� �����մϴ�.");
        AtkEffect.ResetTookDamage(); // tookDamage ����
    }
}
