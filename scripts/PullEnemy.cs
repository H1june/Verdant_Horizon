using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PullEnemy : MonoBehaviour
{
    public float attractionSpeed = 5f; // 끌어당기는 속도
    public float maxAttractionDistance = 3f; // 끌어당길 최대 거리

    private Transform player;
    private Transform monsterTransform;

    private bool isAttracting = false;

    private void OnTriggerEnter2D(Collider2D other) // 2D 콜리더에 맞게 수정
    {
        // "Monster"와 충돌했을 때만 처리
        if (other.CompareTag("Monster"))
        {
            monsterTransform = other.transform;
            isAttracting = true; // 끌어당기기 시작
        }
    }

    private void Update()
    {
        player = GameObject.FindWithTag("Player").transform; // 플레이어 위치

        if (isAttracting && monsterTransform != null)
        {
            // x축 방향 계산
            int direction = player.position.x > monsterTransform.position.x ? 1 : -1;

            // 이동할 거리 계산
            float moveDistance = attractionSpeed * Time.deltaTime;

            // 몬스터 위치 업데이트 (x축 방향만 이동)
            monsterTransform.position = new Vector3(
                monsterTransform.position.x + (direction * moveDistance),
                monsterTransform.position.y,
                monsterTransform.position.z
            );

            // 끌어당기기 종료 조건: 일정 거리 이하로 가까워질 때
            if (Mathf.Abs(monsterTransform.position.x - player.position.x) <= maxAttractionDistance)
            {
                isAttracting = false; // 끌어당기기 종료
            }
        }
    }

}