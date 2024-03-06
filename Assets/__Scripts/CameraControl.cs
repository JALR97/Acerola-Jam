using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float camSpeed;
 
    private void Update()
    {
        var moveDirection = player.position - transform.position;
        moveDirection.z = 0;
        transform.Translate(moveDirection.normalized * (Time.deltaTime * camSpeed));
    }
}
