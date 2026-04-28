using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("기본 설정")]
    public Transform target;     // 카메라가 따라다니고 회전할 중심점 (캐릭터)
    public float distance = 2f;  // 캐릭터와 카메라 사이의 거리
    public float mouseSen = 2f;  // 마우스 감도

    [Header("락온 설정")]
    public bool isLockedOn = false;
    public Transform lockTarget;
    public float lockOnRange = 20f;
    public LayerMask enemyLayer;

    // 락온 카메라가 부드럽게 이동하는 속도 (숫자가 작을수록 묵직하고 느림)
    public float lockOnSpeed = 10f;

    private float mouseX;
    private float mouseY;

    private void Start()
    {
        // 게임 시작 시 마우스 커서를 화면 중앙에 고정하고 숨김 (선택 사항)
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse2) || Input.GetKeyDown(KeyCode.Q))
        {
            ToggleLockOn();
        }

        if (isLockedOn )
        {
            // 마우스 휠을 굴렸는지 감지 (0.01 이상이면 위로, -0.01 이하면 아래로)
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                // 휠을 내리면(음수) 다음 타겟(+1), 올리면(양수) 이전 타겟(-1)
                SwitchLockOnTarget(scroll < 0 ? 1 : -1);
            }

        }
        else
        {
            // 1. 마우스 입력 누적 (Time.deltaTime 안 곱함!)
            mouseX += Input.GetAxisRaw("Mouse X") * mouseSen;
            mouseY -= Input.GetAxisRaw("Mouse Y") * mouseSen;

            // 3인칭 특성상 카메라가 땅을 뚫고 들어가는 걸 막기 위해 하단 각도를 빡빡하게 제한해야 해 (-20도 정도)
            mouseY = Mathf.Clamp(mouseY, -20f, 80f);
        }

        
    }

    private void LateUpdate()
    {
        // 타겟이 없으면 에러가 나니까 예외 처리! (확실하게 해둬야 해)
        if (target == null) return;

        if (isLockedOn && lockTarget != null)
        {
            // 1. 락온 상태의 카메라 연산

            // 타겟과의 거리가 너무 멀어지면 락온 강제 해제 (자동 풀림 로직)
            if (Vector3.Distance(target.position, lockTarget.position) > lockOnRange + 5f)
            {
                ToggleLockOn();
                return;
            }

            // 플레이어에서 보스를 향하는 방향 벡터
            Vector3 dirToTarget = (lockTarget.position - target.position).normalized;

            // 잼민이표 멀미 방지 방어막: 락온이 풀리는 순간을 위해 마우스 변수를 실시간 동기화해둠!
            Quaternion lookRotation = Quaternion.LookRotation(dirToTarget);
            float targetX = lookRotation.eulerAngles.y;
            float targetY = 15f; // 보스를 살짝 올려다보는 얼짱 각도 고정

            mouseX = Mathf.LerpAngle(mouseX, targetX, Time.deltaTime * lockOnSpeed);
            mouseY = Mathf.Lerp(mouseY, targetY, Time.deltaTime * lockOnSpeed);

            // 위치 계산 (보스를 바라보는 각도 기준으로 뒤로 빼기)
            Quaternion rotation = Quaternion.Euler(mouseY, mouseX, 0f);
            Vector3 reverseDistance = new Vector3(0.0f, 0.0f, -distance);
            transform.position = target.position + rotation * reverseDistance;

            // 카메라는 보스를 강제로 쳐다봄!
            // (진짜 프로들은 플레이어와 적의 중간(1/2) 지점을 보게 하지만, 일단 보스를 보게 고정)
            // 3. 렌즈(시선)가 보스를 바라보는 것도 부드럽게 Slerp로 처리!
            Quaternion targetLensRotation = Quaternion.LookRotation(lockTarget.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetLensRotation, Time.deltaTime * lockOnSpeed);
        }
        else
        {
            //  2. 평상시 자유 카메라 연산 (네가 짰던 완벽한 기존 로직)
            Quaternion rotation = Quaternion.Euler(mouseY, mouseX, 0f);
            Vector3 reverseDistance = new Vector3(0.0f, 0.0f, -distance);

            transform.position = target.position + rotation * reverseDistance;
            transform.LookAt(target.position + Vector3.up * 1f);
        }
    }

    private void ToggleLockOn()
    {
        if (isLockedOn)
        {
            isLockedOn = false;
            lockTarget = null;
        }
        else
        {
            FindInitialLockOnTarget();
        }
    }

    private void FindInitialLockOnTarget()
    {
        Collider[] enemies = Physics.OverlapSphere(target.position, lockOnRange, enemyLayer);
        if (enemies.Length == 0) return;

        float closestDist = Mathf.Infinity;
        Transform bestTarget = null;

        // 가장 가까운 적을 첫 타겟으로 지정
        foreach (Collider enemy in enemies)
        {
            float dist = Vector3.Distance(target.position, enemy.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                bestTarget = enemy.transform;
            }
        }

        if (bestTarget != null)
        {
            isLockedOn = true;
            lockTarget = bestTarget;
        }


    }

    private void SwitchLockOnTarget(int direction)
    {
        Collider[] enemies = Physics.OverlapSphere(target.position, lockOnRange, enemyLayer);
        if (enemies.Length <= 1) return; // 혼자밖에 없으면 바꿀 필요 없음

        // 배열을 리스트로 변환
        List<Transform> validEnemies = new List<Transform>();
        foreach (Collider col in enemies) validEnemies.Add(col.transform);

        // 플레이어와의 거리를 기준으로 오름차순 정렬 (가까운 순서)
        validEnemies.Sort((a, b) => Vector3.Distance(target.position, a.position).CompareTo(Vector3.Distance(target.position, b.position)));

        // 현재 타겟이 리스트의 몇 번째(Index)인지 찾기
        int currentIndex = validEnemies.IndexOf(lockTarget);
        if (currentIndex == -1) currentIndex = 0;

        // 휠 방향에 따라 다음 인덱스로 이동
        currentIndex += direction;

        // 리스트 끝에 도달하면 처음으로, 처음에서 이전으로 가면 끝으로 (순환 로직)
        if (currentIndex >= validEnemies.Count) currentIndex = 0;
        if (currentIndex < 0) currentIndex = validEnemies.Count - 1;

        // 새로운 타겟 적용!
        lockTarget = validEnemies[currentIndex];
        Debug.Log($"락온 타겟 변경: {lockTarget.name}");
    }
}
