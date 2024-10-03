using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class FreeMoveController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float lookSpeed = 2f;
    public float tiltAngle = 80f;

    private float yaw;
    private float pitch;

    void Update()
    {
        // Mouse Look
        yaw += lookSpeed * Input.GetAxis("Mouse X");
        pitch -= lookSpeed * Input.GetAxis("Mouse Y");
        pitch = Mathf.Clamp(pitch, -tiltAngle, tiltAngle);

        transform.eulerAngles = new Vector3(pitch, yaw, 0);

        // Movement
        float moveDirectionY = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
        float moveDirectionX = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        Vector3 move = transform.right * moveDirectionX + transform.forward * moveDirectionY;

        transform.position += move;
    }
}

