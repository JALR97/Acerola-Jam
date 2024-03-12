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
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [SerializeField] private Sprite[] MC_sprites;
    
    private bool _canWalk = true;
    
    private Vector2 inputVector = new Vector2();
    private Quaternion targetRotation;
    private Vector3 mousePosition;

    public void AimSwitch() {
        _canWalk = !_canWalk;
    }
    
    void Update()
    {
        inputVector.x = Input.GetAxisRaw("Horizontal");
        inputVector.y = Input.GetAxisRaw("Vertical");
        if (inputVector.magnitude > 0 && _canWalk)
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

        if (rotation_GO.transform.up.x < 0) 
            _spriteRenderer.sprite = MC_sprites[0];
        else
            _spriteRenderer.sprite = MC_sprites[1];
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(mousePosition, 0.5f);
    }
}
