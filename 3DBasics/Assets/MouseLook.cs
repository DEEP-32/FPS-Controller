using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    float mouseX;
    float mouseY;
    private float xRotation = 0f;

    public float mouseSensetivity = 180f;
    public Transform playerBody;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; 
       
    }

    // Update is called once per frame
    void Update()
    {
        float tempX = Input.GetAxis("Mouse X");
        float tempY = Input.GetAxis("Mouse Y");

        mouseX = tempX*Time.deltaTime*mouseSensetivity;
        mouseY = tempY*Time.deltaTime*mouseSensetivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 0f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        playerBody.Rotate(Vector3.up * mouseX);
    }
}
