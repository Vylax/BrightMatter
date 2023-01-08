using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float sensitivity = 10f;

    public Transform playerBody;

    private float xRotation = 0f;
    
    public bool canTurnAround = true;

    private void Start()
    {
        ToggleLook(true);
    }

    private void Update()
    {
        if (!canTurnAround)
        {
            return;
        }

        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        playerBody.Rotate(Vector3.up * mouseX);

    }

    public void ToggleLook(bool value)
    {
        //sensitivity = PlayerPrefs.GetFloat("Sensitivity");
        if (value)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            canTurnAround = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            canTurnAround = false;
        }
    }
}
