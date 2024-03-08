using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    [SerializeField] private float walkSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private Transform rotation_GO;
    [SerializeField] private Camera cam;
    [SerializeField] private Vision vs;
    
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
            transform.Translate(rotation_GO.TransformDirection(inputVector) * (walkSpeed * Time.deltaTime));
        }
        
        var mouseVector = new Vector3(Input.mousePosition.x, Input.mousePosition.y, -cam.transform.position.z);
        mousePosition = cam.ScreenToWorldPoint(mouseVector);

        targetRotation = Quaternion.LookRotation(mousePosition - transform.position, Vector3.back);
        targetRotation.x = 0.0f;
        targetRotation.y = 0.0f;
        
        rotation_GO.rotation = Quaternion.Lerp (rotation_GO.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(mousePosition, 0.5f);
    }
}
