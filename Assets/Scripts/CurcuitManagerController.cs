using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CircuitManager))]
public class CurcuitManagerController : Editor {
    public override void OnInspectorGUI() {
        //Called whenever the inspector is drawn for this object.
        DrawDefaultInspector();

        CircuitManager circuitManager = (CircuitManager)FindObjectOfType(typeof(CircuitManager));

        if (GUILayout.Button("Update Road")) {
            if (circuitManager.circuitMesh != null) {
                circuitManager.updatePath(circuitManager.circuitMesh);
                Debug.Log("road Update");
            } else {
                Debug.LogError("circuitMesh required");
            }
        }
    }
}

