using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DrawManager : MonoBehaviour {
    [SerializeField] private GameObject linePrefab;
    private LineRenderer lineOne, lineTwo;
    
    public void DrawLines() {
        lineOne = Instantiate(linePrefab, transform).GetComponent<LineRenderer>();
        lineTwo = Instantiate(linePrefab, transform).GetComponent<LineRenderer>();
    }

    public void UpdateLine(int lineNum, Vector3 point1, Vector3 point2) {
        switch (lineNum) {
            case 1:
                lineOne.SetPosition(0, point1);
                lineOne.SetPosition(1, point2); 
                break;
            case 2:
                lineTwo.SetPosition(0, point1);
                lineTwo.SetPosition(1, point2); 
                break;
        }
    }

    public void ClearLines() {
        //If there's time, fade them
        Destroy(lineOne.gameObject);
        Destroy(lineTwo.gameObject);
    }
}
