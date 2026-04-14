using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;     // 카메라가 따라다니고 회전할 중심점 (캐릭터)
    public float distance = 5f;  // 캐릭터와 카메라 사이의 거리
    public float mouseSen = 2f;  // 마우스 감도

    private float mouseX;
    private float mouseY;

    private void Start()
    {
        // 게임 시작 시 마우스 커서를 화면 중앙에 고정하고 숨김 (선택 사항)
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // 1. 마우스 입력 누적 (Time.deltaTime 안 곱함!)
        mouseX += Input.GetAxisRaw("Mouse X") * mouseSen;
        mouseY -= Input.GetAxisRaw("Mouse Y") * mouseSen;

        // 3인칭 특성상 카메라가 땅을 뚫고 들어가는 걸 막기 위해 하단 각도를 빡빡하게 제한해야 해 (-20도 정도)
        mouseY = Mathf.Clamp(mouseY, -20f, 80f);
    }

    private void LateUpdate()
    {
        // 타겟이 없으면 에러가 나니까 예외 처리! (확실하게 해둬야 해)
        if (target == null) return;

        // 2. 누적된 마우스 값으로 회전(Quaternion) 계산
        Quaternion rotation = Quaternion.Euler(mouseY, mouseX, 0f);

        // 3. 거리 계산: 타겟 위치에서 Z축을 뒤로(distance만큼) 빼줌
        Vector3 reverseDistance = new Vector3(0.0f, 0.0f, -distance);

        // 4. 최종 위치 적용: 타겟의 위치 + 회전값이 적용된 거리
        transform.position = target.position + rotation * reverseDistance;

        // 5. 카메라는 항상 타겟을 쳐다보게 고정
        transform.LookAt(target);
    }
}
