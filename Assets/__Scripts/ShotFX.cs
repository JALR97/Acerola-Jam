using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotFX : MonoBehaviour {
    
    [SerializeField] private Animator _animator;
    private float timer = 0;
    [SerializeField] private float lifetime;
    private void Update() {
        timer += Time.deltaTime;
        if (timer >= lifetime) {
            Destroy(gameObject);
        }
    }
}
