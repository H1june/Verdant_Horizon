using System.Collections;
using UnityEngine;

public class SwordAuraEffect : MonoBehaviour
{
    public float moveSpeed = 5f;  // 검기 이동 속도
    public float lifeTime = 3f;   // 검기의 생명 시간 (몇 초 뒤에 사라짐)
    public Vector3 direction;     // 플레이어의 이동 방향
    public float range = 10f;     // 검기의 범위 (최대 거리)
    Enemy enemy;

    private void Start()
    {
        // 기본적으로 플레이어가 보는 방향 설정
        direction = transform.localScale.x > 0 ? Vector3.right : Vector3.left;

        // 태그에 따라 방향 조정
        if (gameObject.CompareTag("AttackEffect"))
        {
            // 플레이어가 보는 방향 유지
            direction = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        }
        else if (gameObject.CompareTag("Enemy_Attack_Effect"))
        {
            // 플레이어의 반대 방향 설정
            direction = transform.localScale.x > 0 ? Vector3.left : Vector3.right;
        }
        // 생명 시간이 지나면 오브젝트 삭제
        Destroy(gameObject, lifeTime);
    }


    private void Update()
    {
        // 검기 이동: 방향에 맞게 일정 속도로 이동
        transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);

        // 이동 범위 계산시 방향에 관계없이 절댓값 사용
        Vector3 movement = direction * range;
        float distanceTraveled = movement.magnitude;

        if (distanceTraveled > range)
        {
            Destroy(gameObject);
        }

    }
}
