using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtendedView : MonoBehaviour {

    [SerializeField] private Camera cam;
    [SerializeField] private Transform player;
    
    //Balance
    [SerializeField] private float inactiveRadius;
    [SerializeField] private float maxExtension;
    [SerializeField] private float viewChangeSpeed;
    
    //Vars
    private Vector3 mouseVector;
    private Vector3 distanceVector;
    private float mouseDistance;

    private void Update() {
        mouseVector = Input.mousePosition; 
        mouseVector.x -= Screen.width/2; 
        mouseVector.y -= Screen.height/2;
        
        mouseDistance = mouseVector.magnitude;
        if (mouseDistance > inactiveRadius) {
            transform.position = player.position +
                                 mouseVector.normalized * Mathf.Min((mouseDistance - inactiveRadius) * viewChangeSpeed, maxExtension);
        }
        else
            transform.position = player.position;
    }
}
