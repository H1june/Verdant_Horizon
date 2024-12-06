using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PullEnemy : MonoBehaviour
{
    public float attractionSpeed = 5f; // ������� �ӵ�
    public float maxAttractionDistance = 3f; // ������ �ִ� �Ÿ�

    private Transform player;
    private Transform monsterTransform;

    private bool isAttracting = false;

    private void OnTriggerEnter2D(Collider2D other) // 2D �ݸ����� �°� ����
    {
        // "Monster"�� �浹���� ���� ó��
        if (other.CompareTag("Monster"))
        {
            monsterTransform = other.transform;
            isAttracting = true; // ������� ����
        }
    }

    private void Update()
    {
        player = GameObject.FindWithTag("Player").transform; // �÷��̾� ��ġ

        if (isAttracting && monsterTransform != null)
        {
            // x�� ���� ���
            int direction = player.position.x > monsterTransform.position.x ? 1 : -1;

            // �̵��� �Ÿ� ���
            float moveDistance = attractionSpeed * Time.deltaTime;

            // ���� ��ġ ������Ʈ (x�� ���⸸ �̵�)
            monsterTransform.position = new Vector3(
                monsterTransform.position.x + (direction * moveDistance),
                monsterTransform.position.y,
                monsterTransform.position.z
            );

            // ������� ���� ����: ���� �Ÿ� ���Ϸ� ������� ��
            if (Mathf.Abs(monsterTransform.position.x - player.position.x) <= maxAttractionDistance)
            {
                isAttracting = false; // ������� ����
            }
        }
    }

}