using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(KartController))]
public class KartBrain : MonoBehaviour {

    [HideInInspector]
    public KartController kartController;

    private void Awake() {
        kartController = GetComponent<KartController>();
    }

    private void Update() {
        kartController.ApplyAcceleration(Random.Range(0.0f, 1.0f));
        kartController.Steer(Random.Range(-1.0f, 1.0f));
    }
}
