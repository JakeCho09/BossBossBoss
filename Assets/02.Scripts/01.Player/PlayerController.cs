using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState { IdleMove, Roll, Hit, Attack, Dead, Sheathing, StopRolling}

    [Header("현재 상태")]
    public PlayerState currentState = PlayerState.IdleMove;

    [Header("이동설정")]
    public float walkSpeed = 2.0f;
    public float runSpeed = 5.0f;
    public float rotationSpeed = 10.0f;
    public float acceleration = 10f;

    [Header("컴포넌트")]
    public Transform cameraTr;
    public Animator animator;
    public Rigidbody rb;

    [Header("구르기 설정")]
    public float rollThrust = 20f;
    public float rollDecay = 4f;
    public float currentRollSpeed;
    public bool isInvicible = false;

    [Header("공격 설정")]
    public int attackCombo = 0;
    public bool nextAttackQueued = false;

    private float currentSpeed = 0f;

    [Header("무기 소켓 설정")]
    public GameObject weapon;
    public Transform handSocket;
    public Transform sheathSocket;

    [Header("공격 이동 설정")]
    public float attackStepThrust = 4f; // 제자리 1타 & 2,3타 연계 시 튕겨나가는 전진 속도
    public float attackDecay = 8f;      // 브레이크를 밟는 힘 (숫자가 클수록 묵직하게 멈춤)
    private float currentAttackSpeed;   // 공격 도중 실시간으로 깎일 현재 속도

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    private void Update()
    {
        switch (currentState)
        {
            case PlayerState.IdleMove:

                PlayerMove();
                HandleActionInput();
                break;

            case PlayerState.Roll:

                HandleRolling();
                break;

            case PlayerState.Attack:
                HandleAttacking();
                break;

            case PlayerState.Hit:
            case PlayerState.Dead:
                break;

        }
    }

    void HandleActionInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && currentState == PlayerState.IdleMove)
        {
            StartRoll();
        }
        else if  (Input.GetMouseButtonDown(0) && currentState == PlayerState.IdleMove)
        {
            StartAttack();
        }
    }

    void StartRoll()
    {
        currentState = PlayerState.Roll;

        isInvicible = true;

        currentRollSpeed = rollThrust;

        animator.CrossFade("Roll", 0.1f, -1, 0f);

        //Debug.Log("구르기 시작");
    }
    void HandleRolling()
    {
        // 2. 매 프레임마다 현재 속도를 0을 향해 부드럽게 깎아버림!
        currentRollSpeed = Mathf.Lerp(currentRollSpeed, 0f, rollDecay * Time.deltaTime);

        // 3. 깎여나간 속도를 캐릭터의 앞방향에 적용
        rb.velocity = new Vector3(transform.forward.x * currentRollSpeed, rb.velocity.y, transform.forward.z * currentRollSpeed);
    }

    
    public void EndRoll()
    {
        currentState = PlayerState.IdleMove;

        isInvicible = false;

        animator.CrossFade("Blend Tree", 0.1f);
        //Debug.Log("구르기 멈춤");
    }

    void StartAttack()
    {
        currentState = PlayerState.Attack;

        attackCombo = 1;
        nextAttackQueued = false;

        if (currentSpeed > 1f)
        {
            // 달리고 있었다면 그 속도를 그대로 공격 속도에 넘겨서 관성을 유지!
            currentAttackSpeed = currentSpeed;
        }
        else
        {
            // 제자리에 서 있었다면 앞으로 살짝 튀어 나가는 스텝 전진력을 부여!
            currentAttackSpeed = attackStepThrust;
        }

        animator.CrossFade("DrawSlash", 0.1f);
    }
    public void DrawWeapon()
    {
        if (currentState != PlayerState.Attack) return;

        weapon.transform.SetParent(handSocket);
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;
    }
    public void SheatheWeapon()
    {
        weapon.transform.SetParent(sheathSocket);
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;
    }
    void FirstSlash()
    {
        if (currentState != PlayerState.Attack) return;
        animator.CrossFade("Attack1", 0.1f);
    }
    void HandleAttacking()
    {
        currentAttackSpeed = Mathf.Lerp(currentAttackSpeed, 0f, attackDecay * Time.deltaTime);
        rb.velocity = new Vector3(transform.forward.x * currentAttackSpeed, rb.velocity.y, transform.forward.z * currentAttackSpeed);

        if (Input.GetKeyDown(KeyCode.Space)) //선입력 전에 구르기 하면 우선 입력
        {
            SheatheWeapon();

            
            StartRoll();

            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            nextAttackQueued = true;
        }
    }
    public void CheckCombo()
    {
        if (currentState != PlayerState.Attack) return;

        if (nextAttackQueued)
        {
            attackCombo++;
            nextAttackQueued = false;

            if (attackCombo == 2)
            {
                currentAttackSpeed = attackStepThrust;
                animator.CrossFade("Attack2", 0.1f);
            }
            else if(attackCombo == 3)
            {
                currentAttackSpeed = attackStepThrust;
                animator.CrossFade("Attack3", 0.1f);
            }
            else
            {
                EndAttack();
            }
        }
        else
        {
            EndAttack();
        }
    }
    void EndAttack()
    {
        currentState = PlayerState.Sheathing;
        attackCombo = 0;
        nextAttackQueued = false;

        animator.CrossFade("Sheathing", 0.1f);
    }

    void FinishSheathing()
    {
        currentState = PlayerState.IdleMove;
        animator.CrossFade("Blend Tree", 0.1f);

    }

    void PlayerMove()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 1. 카메라 기준의 입력 방향(moveInput) 먼저 계산
        Vector3 camForward = cameraTr.forward;
        Vector3 camRight = cameraTr.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveInput = (camForward * v + camRight * h).normalized;

        float targetSpeed = 0f;

        // 2. 입력이 있을 때만 회전하고, 목표 속도 세팅!
        if (moveInput.sqrMagnitude > 0f)
        {
            targetSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

            // 에러 해결의 핵심: 입력 방향이 (0,0,0)이 아닐 때만 LookRotation 실행!
            Quaternion targetRotation = Quaternion.LookRotation(moveInput);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // 3. 속도는 항상 Lerp로 부드럽게 가속/감속
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.deltaTime);

        // 4. 이동은 moveInput(키보드 방향)이 아니라 transform.forward(캐릭터가 쳐다보는 앞방향)으로!
        // 이렇게 해야 키보드를 떼서 moveInput이 0이 되어도, 캐릭터가 마지막으로 보던 앞방향으로 서서히 미끄러지며 멈춤.
        if (currentSpeed > 0.1f)
        {
            rb.velocity = new Vector3(transform.forward.x * currentSpeed, rb.velocity.y, transform.forward.z * currentSpeed);
        }
        else
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }

        // 5. 애니메이션 동기화
        animator.SetFloat("Speed", currentSpeed);
    }
}
