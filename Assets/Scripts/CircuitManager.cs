using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteAlways, RequireComponent(typeof(CircuitMesh))]
public class CircuitManager : MonoBehaviour {

    public PathCreation.PathCreator path = null;

    private void OnValidate() {
        CircuitMesh circuitMesh = GetComponent<CircuitMesh>();
        EditorApplication.delayCall += () => updatePath(circuitMesh);
    }

    private void updatePath(CircuitMesh circuitMesh) {
        if (!Application.isPlaying) {
            if (path != null) {
                if (circuitMesh != null) {
                    circuitMesh.UpdatePath(path);
                } else {
                    Debug.Log("circuitMesh can't be found");
                }
            } else {
                Debug.LogWarning("A path must be assign in CircuitManager");
            }
        }
    }
}
