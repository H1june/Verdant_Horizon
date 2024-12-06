using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
     
    public float smoothSpeed = 3;
    public Vector2 offset;
    public float limitMinX, limitMaxX, limitMinY, limitMaxY;
    float cameraHalfWidth, cameraHalfHeight;

    private Vector3 originalPosition;  // 카메라의 초기 위치를 저장하기 위한 변수

    private void Start()
    {
        cameraHalfWidth = Camera.main.aspect * Camera.main.orthographicSize;
        cameraHalfHeight = Camera.main.orthographicSize;

        // 카메라의 초기 위치를 저장
        originalPosition = transform.position;
    }

    private void LateUpdate()
    {
        // target이 null인지 확인하여, 파괴된 경우 카메라가 추적하지 않도록 처리
        if (target == null)
        {
            Debug.LogWarning("Target object is destroyed, camera will no longer follow it.");
            return;
        }

        // 타겟의 y좌표가 -10 이하인 경우, 카메라를 초기 위치로 되돌림
        if (target.position.y < -10.0f)
        {
            ResetCameraPosition();
            return;  // 카메라 위치를 초기화한 후에는 나머지 코드를 실행하지 않음
        }

        Vector3 desiredPosition = new Vector3(
            Mathf.Clamp(target.position.x + offset.x, limitMinX + cameraHalfWidth, limitMaxX - cameraHalfWidth),   // X
            Mathf.Clamp(target.position.y + offset.y, limitMinY + cameraHalfHeight, limitMaxY - cameraHalfHeight), // Y
            -10);                                                                                                  // Z

        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);
    }


    // 카메라를 초기 위치로 되돌리는 메서드
    private void ResetCameraPosition()
    {
        transform.position = originalPosition;  // 초기 위치로 이동
    }
}
