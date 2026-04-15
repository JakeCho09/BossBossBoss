using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 2;
    public float rotationSpeed = 10f;
    public Transform cameraTr;

    private void Update()
    {
        PlayerMove();
    }

    void PlayerMove()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

 
        if (h == 0 && v == 0) return;


        Vector3 camForward = cameraTr.forward;
        Vector3 camRight = cameraTr.right;

   
        camForward.y = 0f;
        camRight.y = 0f;


        camForward.Normalize();
        camRight.Normalize();


        Vector3 moveDirection = (camForward * v + camRight * h).normalized;


        transform.position += moveDirection * moveSpeed * Time.deltaTime;


        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);


    }
}
