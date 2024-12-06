using System.Collections;
using UnityEngine;

public class SwordAuraEffect : MonoBehaviour
{
    public float moveSpeed = 5f;  // �˱� �̵� �ӵ�
    public float lifeTime = 3f;   // �˱��� ���� �ð� (�� �� �ڿ� �����)
    public Vector3 direction;     // �÷��̾��� �̵� ����
    public float range = 10f;     // �˱��� ���� (�ִ� �Ÿ�)
    Enemy enemy;

    private void Start()
    {
        // �⺻������ �÷��̾ ���� ���� ����
        direction = transform.localScale.x > 0 ? Vector3.right : Vector3.left;

        // �±׿� ���� ���� ����
        if (gameObject.CompareTag("AttackEffect"))
        {
            // �÷��̾ ���� ���� ����
            direction = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        }
        else if (gameObject.CompareTag("Enemy_Attack_Effect"))
        {
            // �÷��̾��� �ݴ� ���� ����
            direction = transform.localScale.x > 0 ? Vector3.left : Vector3.right;
        }
        // ���� �ð��� ������ ������Ʈ ����
        Destroy(gameObject, lifeTime);
    }


    private void Update()
    {
        // �˱� �̵�: ���⿡ �°� ���� �ӵ��� �̵�
        transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);

        // �̵� ���� ���� ���⿡ ������� ���� ���
        Vector3 movement = direction * range;
        float distanceTraveled = movement.magnitude;

        if (distanceTraveled > range)
        {
            Destroy(gameObject);
        }

    }
}
