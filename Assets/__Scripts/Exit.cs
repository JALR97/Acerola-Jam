using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exit : MonoBehaviour {
    [SerializeField] private GameObject Prompt;
    private bool ended = false;
    private void OnTriggerEnter2D(Collider2D col) {
        if (col.gameObject.CompareTag("Player")) {
            col.gameObject.SetActive(false);
            Debug.Log("EXIT REACHED, BYE FELICIA");
            Prompt.SetActive(true);
            ended = true;
        }
    }

    private void Update() {
        if (ended && Input.GetKeyDown(KeyCode.Escape)) {
            //Return to menu
            Debug.Log("Return to menu");
        }
    }
}
