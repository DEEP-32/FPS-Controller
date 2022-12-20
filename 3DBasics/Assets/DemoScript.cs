using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoScript : MonoBehaviour
{
    float speed = 5f;
    Vector3 moveDirection;
    public Transform fpsCam;
    public CharacterController controller;
    float rotationX = 0f;

    float gravity = 30f;
    [SerializeField]float jumpForce = 8f;


    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void Update()
    {
        handleMoveInputs();
        handleMouseInputs();
        /*  if (Input.GetKeyDown(KeyCode.Space))
          {
              Debug.Log("Jump key is pressed");
          }
          if (controller.isGrounded)
          {
              Debug.Log("On the ground");
          }*/
        bool canJump = Input.GetKeyDown(KeyCode.Space) && controller.isGrounded;
        //Debug.Log(canJump);
        if (canJump)
        {
            //Debug.Log("Inside if statement");
            handleJump();
        }

        applyFinalMovements();
        
    }

    private void handleMoveInputs()
    {
        float x = Input.GetAxisRaw("Horizontal") * speed;
        float z = Input.GetAxisRaw("Vertical") * speed;

        float tempY = moveDirection.y;

        moveDirection = transform.right * x + transform.forward * z;

        moveDirection.y = tempY;

    }

    private void handleMouseInputs()
    {
        float tempX = Input.GetAxis("Mouse X");
        float tempY = Input.GetAxis("Mouse Y");

        float mouseX = tempX * 400f *Time.deltaTime;
        float mouseY = tempY * 400f *Time.deltaTime;


        rotationX  -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);

        fpsCam.localRotation = Quaternion.Euler(rotationX, 0, 0);
       
        transform.rotation *= Quaternion.Euler(0,mouseX,0);
    }

    private void applyFinalMovements()
    {
        if (!controller.isGrounded)
        {
          //  Debug.Log("Inside apllyfinal movement");
            moveDirection.y -= gravity * Time.deltaTime;
        }
        controller.Move(moveDirection*Time.deltaTime);
    }

    private void handleJump()
    {
        //Debug.Log("Inside handle jump");
        moveDirection.y = jumpForce;
    }
}
