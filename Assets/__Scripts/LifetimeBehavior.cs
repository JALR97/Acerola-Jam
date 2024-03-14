using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifetimeBehavior : MonoBehaviour {
    
    private float timer = 0;
    public bool startPrompt = false;
    [SerializeField] private GameObject playerRotation;
    
    [SerializeField] private float lifetime;
    private void Update() {
        timer += Time.deltaTime;
        if (timer >= lifetime) {
            if (startPrompt) {
                playerRotation.SetActive(true);
            }
            Destroy(gameObject);
        }
    }
}
