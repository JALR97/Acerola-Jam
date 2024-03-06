using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    [SerializeField] private float walkSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private Camera cam;
    
    private Vector2 inputVector = new Vector2();
    private Quaternion targetRotation;
    private Vector3 mousePosition;
    void Update()
    {
        inputVector.x = Input.GetAxisRaw("Horizontal");
        inputVector.y = Input.GetAxisRaw("Vertical");
        if (inputVector.magnitude > 0)
        {
            inputVector.Normalize();
            transform.Translate(inputVector * (walkSpeed * Time.deltaTime));
        }
        
        var mouseVector = new Vector3(Input.mousePosition.x, Input.mousePosition.y, -cam.transform.position.z);
        mousePosition = cam.ScreenToWorldPoint(mouseVector);

        targetRotation = Quaternion.LookRotation(mousePosition - transform.position, Vector3.back);
        targetRotation.x = 0.0f;
        targetRotation.y = 0.0f;
        
        transform.rotation = Quaternion.Lerp (transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(mousePosition, 0.5f);
    }
}
