using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private CharacterController controller;
    [SerializeField] private float speed;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float jumpHieght =3f;
    private bool isGrounded;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;


    Vector3 velocity;
    public float gravity = -9.81f;
    float x, z;

    private void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
       
        if(isGrounded && velocity.y < 0)
        {
            velocity.y= -2f;
        }
        
        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");

        Vector3 dir = transform.right * x + transform.forward * z;
        Vector3 move = dir * speed * Time.deltaTime;
        
        controller.Move(move);

        if(Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHieght*-2f*gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity*Time.deltaTime);
    }
}
