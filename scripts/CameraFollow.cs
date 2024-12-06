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

    private Vector3 originalPosition;  // ī�޶��� �ʱ� ��ġ�� �����ϱ� ���� ����

    private void Start()
    {
        cameraHalfWidth = Camera.main.aspect * Camera.main.orthographicSize;
        cameraHalfHeight = Camera.main.orthographicSize;

        // ī�޶��� �ʱ� ��ġ�� ����
        originalPosition = transform.position;
    }

    private void LateUpdate()
    {
        // target�� null���� Ȯ���Ͽ�, �ı��� ��� ī�޶� �������� �ʵ��� ó��
        if (target == null)
        {
            Debug.LogWarning("Target object is destroyed, camera will no longer follow it.");
            return;
        }

        // Ÿ���� y��ǥ�� -10 ������ ���, ī�޶� �ʱ� ��ġ�� �ǵ���
        if (target.position.y < -10.0f)
        {
            ResetCameraPosition();
            return;  // ī�޶� ��ġ�� �ʱ�ȭ�� �Ŀ��� ������ �ڵ带 �������� ����
        }

        Vector3 desiredPosition = new Vector3(
            Mathf.Clamp(target.position.x + offset.x, limitMinX + cameraHalfWidth, limitMaxX - cameraHalfWidth),   // X
            Mathf.Clamp(target.position.y + offset.y, limitMinY + cameraHalfHeight, limitMaxY - cameraHalfHeight), // Y
            -10);                                                                                                  // Z

        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);
    }


    // ī�޶� �ʱ� ��ġ�� �ǵ����� �޼���
    private void ResetCameraPosition()
    {
        transform.position = originalPosition;  // �ʱ� ��ġ�� �̵�
    }
}
