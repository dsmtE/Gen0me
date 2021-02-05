using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(KartController)), RequireComponent(typeof(RayCastSensors))]
public class KartBrain : MonoBehaviour {

    [HideInInspector]
    public KartController kartController;
    public RayCastSensors rayCastSensors;

    private void Awake() {
        kartController = GetComponent<KartController>();
        rayCastSensors = GetComponent<RayCastSensors>();
    }

    private void Update() {
        var hits = rayCastSensors.GetHitInformations();
        kartController.ApplyAcceleration(aiModel.evalAcceleration(hits[0].distance, hits[1].distance, hits[2].distance));
        kartController.Steer(aiModel.evalSteer(hits[0].distance, hits[1].distance, hits[2].distance));
    }

    private AIModel aiModel;
}
