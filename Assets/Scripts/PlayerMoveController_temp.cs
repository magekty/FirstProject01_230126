using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveController : MonoBehaviour
{

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotateSpeed = 5f;
    [SerializeField] private float mouseSpeed = 5f;

    [SerializeField] private float jumpPower = 5f;

    private Camera cm = null;
    private CharacterController cc = null;

    private float gravity = -9.8f;
    private float mouseY = 0f;
    private float mouseX = 0f;
    private float beforePosY = 0f;

    private bool isJump = false;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        cm = Camera.main;
    }

    private void Update()
    {
        Moveprocess();
    }

    private void Moveprocess()
    {
        float axisH = Input.GetAxis("Horizontal");
        float axisV = Input.GetAxis("Vertical");

        mouseX += Input.GetAxis("Mouse X") * rotateSpeed;
        mouseY += Input.GetAxis("Mouse Y") * mouseSpeed;
        mouseY = Mathf.Clamp(mouseY, -55f, 55f); 

        cm.transform.eulerAngles = new Vector3(-mouseY, mouseX, 0f); 
        transform.eulerAngles = new Vector3(0, mouseX, 0f);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (cc.isGrounded == true)
            {
                isJump = true;
                beforePosY = transform.position.y;
            }
        }
        if (isJump)
        {
            gravity = jumpPower;
            if (transform.position.y >= jumpPower + beforePosY)
            {
                isJump = false;
            }
        }
        else if (!isJump || cc.isGrounded == false)
        {
            gravity += -9.8f * Time.deltaTime;
        }
        else
        {
            gravity = 0f;
        }
        Vector3 position = new Vector3(axisH, gravity, axisV);
        cc.Move(transform.TransformDirection(position) * Time.deltaTime * moveSpeed);

    }

}
