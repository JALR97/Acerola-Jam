using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.XR;

[CustomEditor(typeof(Vision))]
public class VisionEditor : Editor
{
    private void OnSceneGUI()
    {
        Vision vision = (Vision)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(vision.transform.position, Vector3.back, Vector3.up, 360, vision.viewRange);
        Vector3 viewAngleA = vision.DirFromAngle(-vision.viewAngle / 2, false);
        Vector3 viewAngleB = vision.DirFromAngle(vision.viewAngle / 2, false);
        
        Handles.DrawLine(vision.transform.position, vision.transform.position + viewAngleA * vision.viewRange);
        Handles.DrawLine(vision.transform.position, vision.transform.position + viewAngleB * vision.viewRange);

        Handles.color = Color.red;
        foreach (Transform visibleTarget in vision.visibleTargets)
        {
            Handles.DrawLine(visibleTarget.position, vision.transform.position);
        }
    }
}
