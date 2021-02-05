using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(KartController)), RequireComponent(typeof(RayCastSensors))]
public class KartBrain : MonoBehaviour {

    [HideInInspector]
    public KartController kartController;
    private RayCastSensors rayCastSensors;

    private AIModel aiModel;

    private void Awake() {
        kartController = GetComponent<KartController>();
        rayCastSensors = GetComponent<RayCastSensors>();
        rayCastSensors.RaysNumber = 3;

        aiModel = new AIModel();
    }

    private void Update() {
        var hits = rayCastSensors.GetHitInformations();
        kartController.ApplyAcceleration(aiModel.evalAcceleration(hits[0].distance, hits[1].distance, hits[2].distance));
        kartController.Steer(aiModel.evalSteer(hits[0].distance, hits[1].distance, hits[2].distance));
    }

    
}
